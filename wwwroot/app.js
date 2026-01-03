(function(){
  // =============================================
  // WebView Bridge
  // =============================================
  // Safe post helper (try/catch guards against host not ready)
  function post(msg){
    try{
      if(window.chrome && window.chrome.webview){
        window.chrome.webview.postMessage(msg);
      }
    }catch{}
  }

  // =============================================
  // Utilities
  // =============================================
  // Normalize MAC to AA:BB:CC:DD:EE:FF (accepts many formats)
  function normalizeMac(mac){
    if(!mac) return '';
    const hex=String(mac).replace(/[^0-9a-fA-F]/g,'').toUpperCase();
    if(hex.length<12) return mac;
    const clean=hex.substring(0,12);
    return clean.match(/.{1,2}/g).join(':');
  }
  // HTML text escape
  function esc(s){ return (''+s).replace(/[&<>]/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;'}[c])); }

  // =============================================
  // Tabs Navigation
  // =============================================
  const tabs=document.querySelectorAll('nav .tab-btn');
  const views=document.querySelectorAll('.view');
  tabs.forEach(btn=>btn.addEventListener('click',()=>{
    // Switch visible view
    tabs.forEach(b=>b.classList.remove('active'));
    btn.classList.add('active');
    views.forEach(v=>v.classList.remove('active'));
    const t=document.getElementById('view-'+btn.dataset.view);
    if(t) t.classList.add('active');

    // Remember last tab
    try{localStorage.setItem('ui_last_view',btn.dataset.view);}catch{}
    post('ui:tab:'+btn.dataset.view);

    // On discovery tab, ensure adapters are fresh
    if(btn.dataset.view==='discovery') post('adapters:request');
  }));
  function activateTabByKey(key){ if(!key) return; const btn=[...tabs].find(b=>b.dataset.view===key); if(btn) btn.click(); }
  try{ const lastView=localStorage.getItem('ui_last_view'); if(lastView) activateTabByKey(lastView);}catch{}

  // =============================================
  // Adapters (Table + Controls)
  // =============================================
  const refreshBtn=document.getElementById('btnRefreshAdapters');
  const adaptersBody=document.querySelector('#tblAdapters tbody');
  const selAdapter=document.getElementById('selAdapter');
  const btnDhcp=document.getElementById('btnDhcp');
  const btnStatic=document.getElementById('btnStatic');
  const favoriteSelect=document.getElementById('favoriteSelect');

  const ip1=document.getElementById('ip1');
  const ip2=document.getElementById('ip2');
  const ip3=document.getElementById('ip3');
  const ip4=document.getElementById('ip4');
  const sub1=document.getElementById('sub1');
  const sub2=document.getElementById('sub2');
  const sub3=document.getElementById('sub3');
  const sub4=document.getElementById('sub4');
  const gw1=document.getElementById('gw1');
  const gw2=document.getElementById('gw2');
  const gw3=document.getElementById('gw3');
  const gw4=document.getElementById('gw4');

  const defSub1=document.getElementById('defSub1');
  const defSub2=document.getElementById('defSub2');
  const defSub3=document.getElementById('defSub3');
  const defSub4=document.getElementById('defSub4');

  // Visibility checkboxes (Settings)
  const setShowVirtual=document.getElementById('setShowVirtual');
  const setShowLoopback=document.getElementById('setShowLoopback');
  const setShowBluetooth=document.getElementById('setShowBluetooth');
  let adaptersCacheRaw='';

  // Persist defaults (unchecked)
  try{
    const v=localStorage.getItem('adapters_show_virtual'); if(setShowVirtual) setShowVirtual.checked = v==='true';
    const l=localStorage.getItem('adapters_show_loopback'); if(setShowLoopback) setShowLoopback.checked = l==='true';
    const b=localStorage.getItem('adapters_show_bluetooth'); if(setShowBluetooth) setShowBluetooth.checked = b==='true';
  }catch{}
  function persistVisibility(){
    try{
      localStorage.setItem('adapters_show_virtual', setShowVirtual && setShowVirtual.checked? 'true':'false');
      localStorage.setItem('adapters_show_loopback', setShowLoopback && setShowLoopback.checked? 'true':'false');
      localStorage.setItem('adapters_show_bluetooth', setShowBluetooth && setShowBluetooth.checked? 'true':'false');
    }catch{}
  }
  function onVisibilityChanged(){
    persistVisibility();
    if(adaptersCacheRaw) handleAdapters(adaptersCacheRaw);
  }
  if(setShowVirtual) setShowVirtual.addEventListener('change', onVisibilityChanged);
  if(setShowLoopback) setShowLoopback.addEventListener('change', onVisibilityChanged);
  if(setShowBluetooth) setShowBluetooth.addEventListener('change', onVisibilityChanged);

  // Clear IP/Subnet/Gateway inputs
  function clearAdapterInputs(){ [ip1,ip2,ip3,ip4,sub1,sub2,sub3,sub4,gw1,gw2,gw3,gw4].forEach(el=>{ if(el) el.value=''; }); }
  // Clamp numeric inputs to 0..255 and max length 3
  function clampOctetInput(el){ if(!el) return; el.addEventListener('input',()=>{ let v=(el.value||'').replace(/[^0-9]/g,''); if(v.length>3) v=v.substring(0,3); let n=parseInt(v||'0',10); if(isNaN(n)) n=0; if(n>255) n=255; el.value = v===''? '': String(n); }); }

  // Auto-advance logic across octets
  const octetOrder=[ip1,ip2,ip3,ip4,sub1,sub2,sub3,sub4,gw1,gw2,gw3,gw4].filter(Boolean);
  function focusNext(from){ const idx=octetOrder.indexOf(from); if(idx>=0 && idx<octetOrder.length-1){ octetOrder[idx+1].focus(); octetOrder[idx+1].select && octetOrder[idx+1].select(); } }
  function wireAdvance(el){ if(!el) return;
    // When 3 digits entered -> move to next
    el.addEventListener('input',()=>{ const v=(el.value||'').trim(); if(v.length===3){ focusNext(el); } });
    // On keydown: Tab moves next (native), Enter or '.' also advance
    el.addEventListener('keydown',(ev)=>{
      if(ev.key==='Enter' || ev.key==='.' ){ ev.preventDefault(); focusNext(el); }
      // Ensure Tab moves next in our defined order even if default tab order differs
      if(ev.key==='Tab'){
        ev.preventDefault(); focusNext(el);
      }
    });
  }

  [ip1,ip2,ip3,ip4,sub1,sub2,sub3,sub4,gw1,gw2,gw3,gw4].forEach(el=>{ clampOctetInput(el); wireAdvance(el); });

  // Refresh adapters
  let refreshing=false;
  if(refreshBtn) refreshBtn.addEventListener('click',()=>{
    if(refreshing) return;
    refreshing=true;
    adaptersBody.innerHTML='<tr class="loading"><td colspan="8">Refreshing...</td></tr>';
    post('adapters:request');
  });

  // DHCP / Static actions
  if(btnDhcp) btnDhcp.addEventListener('click',()=>{ const adapter=selAdapter.value; post('adapters:setDhcp:'+adapter); });
  if(btnStatic) btnStatic.addEventListener('click',()=>{
    const adapter=selAdapter.value;
    const o1=(ip1&&ip1.value||'').trim(); const o2=(ip2&&ip2.value||'').trim(); const o3=(ip3&&ip3.value||'').trim(); const o4=(ip4&&ip4.value||'').trim();
    const s1=(sub1&&sub1.value||'').trim(); const s2=(sub2&&sub2.value||'').trim(); const s3=(sub3&&sub3.value||'').trim(); const s4=(sub4&&sub4.value||'').trim();
    const g1=(gw1&&gw1.value||'').trim(); const g2=(gw2&&gw2.value||'').trim(); const g3=(gw3&&gw3.value||'').trim(); const g4=(gw4&&gw4.value||'').trim();
    const ip = (o1&&o2&&o3&&o4) ? `${o1}.${o2}.${o3}.${o4}` : '';
    const mask = (s1&&s2&&s3&&s4) ? `${s1}.${s2}.${s3}.${s4}` : '';
    const gw = (g1&&g2&&g3&&g4) ? `${g1}.${g2}.${g3}.${g4}` : '';
    post('adapters:setStatic:'+adapter+'|'+ip+'|'+mask+'|'+gw);
  });

  // Favorite select -> fill inputs
  if(favoriteSelect) favoriteSelect.addEventListener('change',()=>{
    const opt=favoriteSelect.selectedOptions[0]; if(!opt||!opt.value) return;
    const ip=(opt.value||'').split('.'); if(ip1) ip1.value = ip[0]||''; if(ip2) ip2.value = ip[1]||''; if(ip3) ip3.value = ip[2]||''; if(ip4) ip4.value = ip[3]||'';
    const subnet=(opt.dataset.subnet||'').split('.'); if(sub1) sub1.value = subnet[0]||''; if(sub2) sub2.value = subnet[1]||''; if(sub3) sub3.value = subnet[2]||''; if(sub4) sub4.value = subnet[3]||'';
    const gws=(opt.dataset.gateway||'').split('.'); if(gw1) gw1.value = gws[0]||''; if(gw2) gw2.value = gws[1]||''; if(gw3) gw3.value = gws[2]||''; if(gw4) gw4.value = gws[3]||'';
  });

  function isVirtualAdapter(a){
    const t = (a.HardwareDetails||'') + ' ' + (a.AdapterName||'');
    return /virtual|hyper-v|vmware|virtualbox|tap|bridge/i.test(t);
  }
  function isLoopbackAdapter(a){
    const name=(a.AdapterName||'');
    const ip=(a.IpAddress||'');
    return /loopback/i.test(name) || ip.startsWith('127.');
  }
  function isBluetoothAdapter(a){
    const t = (a.HardwareDetails||'') + ' ' + (a.AdapterName||'');
    return /bluetooth/i.test(t);
  }
  function shouldIncludeAdapter(a){
    const showVirtual = setShowVirtual && setShowVirtual.checked;
    const showLoopback = setShowLoopback && setShowLoopback.checked;
    const showBluetooth = setShowBluetooth && setShowBluetooth.checked;
    if(!showVirtual && isVirtualAdapter(a)) return false;
    if(!showLoopback && isLoopbackAdapter(a)) return false;
    if(!showBluetooth && isBluetoothAdapter(a)) return false;
    return true;
  }

  // Handle adapters:data from host -> populate table & dropdowns
  function handleAdapters(json){
    adaptersCacheRaw = json;
    try{
      const list=JSON.parse(json);
      const filtered = Array.isArray(list)? list.filter(shouldIncludeAdapter) : [];
      const current=selAdapter.value;
      const frag=document.createDocumentFragment();
      if(!Array.isArray(filtered)||filtered.length===0){
        const tr=document.createElement('tr'); tr.innerHTML='<td colspan="8">No adapters found.</td>'; frag.appendChild(tr);
      }else{
        filtered.forEach(a=>{
          const macNorm = normalizeMac(a.MacAddress);
          const tr=document.createElement('tr');
          tr.innerHTML='<td>'+esc(a.AdapterName)+'</td><td>'+esc(a.IsDhcp)+'</td><td>'+esc(a.IpAddress)+'</td><td>'+esc(a.Subnet)+'</td><td>'+esc(a.Gateway)+'</td><td>'+esc(a.Status)+'</td><td>'+esc(a.HardwareDetails)+'</td><td>'+esc(macNorm)+'</td>';
          tr.addEventListener('click',()=>{
            // Row select -> sync dropdowns
            [...adaptersBody.querySelectorAll('tr')].forEach(r=>r.classList.remove('selected'));
            tr.classList.add('selected');
            selAdapter.value=a.AdapterName;
            try{localStorage.setItem('ui_last_adapter',a.AdapterName);}catch{}
            if(discAdapter){
              const opt=[...discAdapter.options].find(o=>o.value===a.AdapterName);
              if(opt){discAdapter.value=a.AdapterName;autoFillRangeFromSelected();
              }
            }
            post('ui:selectedAdapter:'+a.AdapterName);
          });
          if(a.AdapterName===current) tr.classList.add('selected');
          frag.appendChild(tr);
        });
      }
      adaptersBody.innerHTML=''; adaptersBody.appendChild(frag);

      // Rebuild main adapter dropdown (prefer last saved value)
      const lastAdapter=(function(){try{return localStorage.getItem('ui_last_adapter')||'';}catch{return ''}})();
      selAdapter.innerHTML='<option value="(select)">(select)</option>';
      if(Array.isArray(filtered)) filtered.forEach(a=> selAdapter.insertAdjacentHTML('beforeend','<option value="'+esc(a.AdapterName)+'">'+esc(a.AdapterName)+' ('+esc(a.IpAddress)+')</option>'));
      if(lastAdapter && Array.isArray(filtered) && filtered.some(a=>a.AdapterName===lastAdapter)) selAdapter.value=lastAdapter; else if(current && Array.isArray(filtered) && filtered.some(a=>a.AdapterName===current)) selAdapter.value=current;

      // Sync discovery adapter dropdown
      if(discAdapter){
        discAdapter.innerHTML='';
        if(Array.isArray(filtered)) filtered.filter(a=>a.IpAddress).forEach(a=>{
          discAdapter.insertAdjacentHTML('beforeend','<option value="'+esc(a.AdapterName)+'" data-ip="'+esc(a.IpAddress)+'" data-subnet="'+esc(a.Subnet)+'">'+esc(a.AdapterName)+' ('+esc(a.IpAddress)+')</option>');
        });
        if(lastAdapter && [...discAdapter.options].some(o=>o.value===lastAdapter)) discAdapter.value=lastAdapter; else if(selAdapter.value && [...discAdapter.options].some(o=>o.value===selAdapter.value)) discAdapter.value=selAdapter.value;
        autoFillRangeFromSelected();
      }
    }catch{
      adaptersBody.innerHTML='<tr><td colspan="8">Adapter JSON parse error.</td></tr>';
    }finally{ refreshing=false; }
  }

  // Favorites (Settings -> presets injected into Adapters tab select)
  function handleFavorites(json){
    try{
      const list=JSON.parse(json);
      favoriteSelect.innerHTML='<option value="">(none)</option>';
      list.forEach(f=>{ if(f.ip){ favoriteSelect.insertAdjacentHTML('beforeend','<option value="'+esc(f.ip)+'" data-subnet="'+esc(f.subnet)+'" data-gateway="'+(f.gateway?esc(f.gateway):'')+'">'+esc(f.ip)+'</option>'); } });
    }catch{}
  }

  // Host -> UI messages
  if(window.chrome && window.chrome.webview) window.chrome.webview.addEventListener('message',e=>{
    const msg=e.data; if(typeof msg!=='string') return;
    if(msg.startsWith('adapters:data:')) handleAdapters(msg.substring('adapters:data:'.length));
    else if(msg.startsWith('favorites:data:')) handleFavorites(msg.substring('favorites:data:'.length));
    else if(msg==='adapters:clearInputs') clearAdapterInputs();
  });

  // =============================================
  // Discovery
  // =============================================
  const discAdapter=document.getElementById('discAdapter');
  const discStartIp=document.getElementById('discStartIp');
  const discEndIp=document.getElementById('discEndIp');
  const discStart=document.getElementById('discStart');
  const discCancel=document.getElementById('discCancel');
  const discSave=document.getElementById('discSave');
  const discClear=document.getElementById('discClear');
  const discProgress=document.getElementById('discProgress');
  const discActive=document.getElementById('discActive');
  const discEta=document.getElementById('discEta');
  const discResultsTbody=document.querySelector('#discResults tbody');
  let discWasCancelled=false;

  // Discovery adapter change -> sync & persist
  if(discAdapter) discAdapter.addEventListener('change',()=>{
    autoFillRangeFromSelected();
    const selected = discAdapter.value; if(!selected) return;
    // Sync main adapters dropdown
    if(selAdapter){ const has=[...selAdapter.options].some(o=>o.value===selected); if(has){ selAdapter.value=selected; } }
    try{ localStorage.setItem('ui_last_adapter', selected); }catch{}
    post('ui:selectedAdapter:'+selected);
    // Mark row selected in table
    try{
      const rows=[...adaptersBody.querySelectorAll('tr')]; rows.forEach(r=>r.classList.remove('selected'));
      const match=rows.find(r=>{ const cell=r.children && r.children[0]; return cell && cell.textContent.trim()===selected; });
      if(match) match.classList.add('selected');
    }catch{}
  });

  // Auto-fill discovery IP range based on adapter IP/subnet
  function autoFillRangeFromSelected(){
    const opt=discAdapter && discAdapter.selectedOptions[0]; if(!opt) return;
    const ip=opt.getAttribute('data-ip'); const subnet=opt.getAttribute('data-subnet'); if(!ip||!subnet) return;
    const ipParts=ip.split('.').map(n=>parseInt(n,10)); const maskParts=subnet.split('.').map(n=>parseInt(n,10));
    if(ipParts.length!==4||maskParts.length!==4||maskParts.some(isNaN)||ipParts.some(isNaN)) return;
    const networkAddr=[]; const broadcastAddr=[];
    for(let i=0;i<4;i++){ networkAddr[i]=ipParts[i]&maskParts[i]; broadcastAddr[i]=networkAddr[i]|(255-maskParts[i]); }
    const firstUsable=[...networkAddr]; const lastUsable=[...broadcastAddr];
    firstUsable[3]++; for(let i=3;i>0;i--){ if(firstUsable[i]>255){ firstUsable[i]=0; firstUsable[i-1]++; } }
    lastUsable[3]--; for(let i=3;i>0;i--){ if(lastUsable[i]<0){ lastUsable[i]=255; lastUsable[i-1]--; } }
    const isPointToPoint=subnet.trim()==='255.255.255.254';
    if(isPointToPoint){ discStartIp.value=networkAddr.join('.'); discEndIp.value=broadcastAddr.join('.'); }
    else { discStartIp.value=firstUsable.join('.'); discEndIp.value=lastUsable.join('.'); }
  }

  // Discovery actions
  if(discStart)  discStart.addEventListener('click',()=>{ discWasCancelled=false; const a=discAdapter.value.trim(); const s=discStartIp.value.trim(); const e=discEndIp.value.trim(); if(!a||!s||!e){ post('log:info:[DISC] Missing inputs.'); return; } post('disc:start:'+a+'|'+s+'|'+e); });
  if(discCancel) discCancel.addEventListener('click',()=>{ discWasCancelled=true; post('disc:cancel'); });
  if(discSave)   discSave.addEventListener('click',()=>{ post('disc:save'); });
  if(discClear)  discClear.addEventListener('click',()=>{ discWasCancelled=false; post('disc:reset'); });

  // Discovery host -> UI messages
  function updateDiscStats(scanned,total,active,eta){ if(discProgress)discProgress.textContent='Progress: '+scanned+' / '+total; if(discActive)discActive.textContent='Active: '+active; if(discEta)discEta.textContent='ETA: '+eta; }
  if(window.chrome && window.chrome.webview) window.chrome.webview.addEventListener('message',e=>{
    const msg=e.data; if(typeof msg!=='string')return;
    if(msg.startsWith('disc:adapters:')){ /* discovery adapter list now comes from adapters:data */ }
    else if(msg==='disc:cancelled'){ discWasCancelled=true; }
    else if(msg.startsWith('disc:result:')){
      if(discWasCancelled) return;
      const json=msg.substring('disc:result:'.length);
      try{
        const r=JSON.parse(json);
        const tr=document.createElement('tr'); tr.classList.add('active-row');
        const macNorm = normalizeMac(r.Mac||'');
        tr.innerHTML='<td>'+esc(r.Ip)+'</td><td>'+esc(r.Hostname||'')+'</td><td>'+esc(macNorm)+'</td><td>'+esc(r.Manufacturer||'')+'</td><td>'+esc(r.Status||'')+'</td>';
        discResultsTbody.appendChild(tr);
      }catch{}
    }
    else if(msg.startsWith('disc:stats:')){
      const parts=msg.substring('disc:stats:'.length).split('|'); if(parts.length>=4) updateDiscStats(parts[0],parts[1],parts[2],parts[3]);
    }
    else if(msg==='disc:clear'){
      discWasCancelled=false; discResultsTbody.innerHTML=''; updateDiscStats(0,0,0,'--:--:--');
    }
  });

  // =============================================
  // Diagnostics
  // =============================================
  const pingTarget=document.getElementById('pingTarget');
  const btnPing=document.getElementById('btnPing');
  const chkPingContinuous=document.getElementById('chkPingContinuous');
  const btnTrace=document.getElementById('btnTrace');
  const traceTarget=document.getElementById('traceTarget');
  const chkTraceResolve=document.getElementById('chkTraceResolve');
  const btnNslookup=document.getElementById('btnNslookup');
  const nsTarget=document.getElementById('nsTarget');
  const btnPathPing=document.getElementById('btnPathPing');
  const pathPingTarget=document.getElementById('pathPingTarget');
  const btnDiagCancel=document.getElementById('btnDiagCancel');

  // Persist small UI toggles in localStorage
  try{
    const savedPingCont=localStorage.getItem('diag_ping_continuous'); if(savedPingCont==='true' && chkPingContinuous) chkPingContinuous.checked=true;
    const savedResolve=localStorage.getItem('diag_trace_resolve'); if(savedResolve==='false' && chkTraceResolve) chkTraceResolve.checked=false;
  }catch{}
  if(chkPingContinuous) chkPingContinuous.addEventListener('change',()=>{ try{ localStorage.setItem('diag_ping_continuous', chkPingContinuous.checked? 'true':'false'); }catch{} });
  if(chkTraceResolve)    chkTraceResolve   .addEventListener('change',()=>{ try{ localStorage.setItem('diag_trace_resolve',   chkTraceResolve.checked? 'true':'false'); }catch{} });

  // Ping/Trace/NS/PathPing actions
  let continuousPingTimer=null; let pingIntervalMs=2000; // default
  let pingRetries=0; // default retries per click
  if(btnPing)     btnPing    .addEventListener('click',()=>{ const t=pingTarget.value.trim(); if(!t) return; if(chkPingContinuous && chkPingContinuous.checked){ if(continuousPingTimer){ stopContinuousPing('[PING] Continuous stopped by user'); return; } startContinuousPing(t); return; } doPingWithRetries(t); });
  if(btnTrace)    btnTrace   .addEventListener('click',()=>{ const t=traceTarget.value.trim(); if(!t) return; post('log:info:[TRACE] Start '+t); post('trace:'+t+'|'+(chkTraceResolve && chkTraceResolve.checked?'resolve':'nresolve')); });
  if(btnNslookup) btnNslookup.addEventListener('click',()=>{ const t=nsTarget.value.trim(); if(!t) return; post('log:info:[NSLOOKUP] Start '+t); post('nslookup:'+t); });
  if(btnPathPing) btnPathPing.addEventListener('click',()=>{ const t=pathPingTarget.value.trim(); if(!t) return; post('log:info:[PATHPING] Start '+t); post('pathping:'+t); });
  if(btnDiagCancel) btnDiagCancel.addEventListener('click',()=>{ if(continuousPingTimer){ stopContinuousPing('[CANCEL] Continuous ping cancelled'); return; } post('diagnostics:cancel'); });

  function startContinuousPing(t){ btnPing.textContent='Stop'; post('log:info:[PING] Continuous start: '+t); doPingWithRetries(t); continuousPingTimer=setInterval(()=>doPingWithRetries(t),pingIntervalMs); }
  function stopContinuousPing(reason){ clearInterval(continuousPingTimer); continuousPingTimer=null; btnPing.textContent='Ping'; post('log:info:'+reason); }
  function doPing(t){ post('ping:'+t); }
  function doPingWithRetries(t){
    const tries = Math.max(0, pingRetries);
    doPing(t);
    for(let i=0;i<tries;i++) setTimeout(()=>doPing(t), (i+1)*250); // spaced quick retries
  }

  // =============================================
  // Settings (Dark Mode, Discovery tuneables, Resolvers, Favorites)
  // =============================================
  let isApplyingSettings=false;
  const setDiscoveryParallel=document.getElementById('setDiscoveryParallel');
  const setDiscoveryTimeout=document.getElementById('setDiscoveryTimeout');
  const settingsStatus=document.getElementById('settingsStatus');
  const setDarkMode=document.getElementById('setDarkMode');
  const setPingDefault=document.getElementById('setPingDefault');
  const setEnableLlmnr=document.getElementById('setEnableLlmnr');
  const setEnableMdns=document.getElementById('setEnableMdns');
  const setEnableNbns=document.getElementById('setEnableNbns');
  const setEnableNbtstat=document.getElementById('setEnableNbtstat');
  // Diagnostics settings elements
  const setPingRetriesEl=document.getElementById('setPingRetries');
  const setPingIntervalEl=document.getElementById('setPingInterval');

  function applyDarkMode(on){ document.body.classList[on? 'add':'remove']('dark'); }
  function saveSettings(){ if(isApplyingSettings) return; // suppress during load
    const parallel=parseInt(setDiscoveryParallel && setDiscoveryParallel.value || '');
    const timeout =parseInt(setDiscoveryTimeout  && setDiscoveryTimeout .value || '');
    const llmnr = setEnableLlmnr  && setEnableLlmnr .checked ? 'llmnr:on' : 'llmnr:off';
    const mdns  = setEnableMdns   && setEnableMdns  .checked ? 'mdns:on'  : 'mdns:off';
    const nbns  = setEnableNbns   && setEnableNbns  .checked ? 'nbns:on'  : 'nbns:off';
    const nbt   = setEnableNbtstat&& setEnableNbtstat.checked ? 'nbt:on'  : 'nbt:off';
    const defSubnet = (window.getDefaultSubnetFromOctets? window.getDefaultSubnetFromOctets(): (document.getElementById('setDefaultSubnet')?.value||'')).trim();
    const dark = setDarkMode && setDarkMode.checked ? 'dark' : '';
    const pingRetriesVal = parseInt(setPingRetriesEl && setPingRetriesEl.value || '');
    const pingIntervalSecVal = parseInt(setPingIntervalEl && setPingIntervalEl.value || '');
    // Apply locally
    if(!isNaN(pingRetriesVal)) { pingRetries = Math.max(0, Math.min(10, pingRetriesVal)); try{ localStorage.setItem('diag_ping_retries', String(pingRetries)); }catch{} }
    if(!isNaN(pingIntervalSecVal)) { const ms=Math.max(1, Math.min(60, pingIntervalSecVal))*1000; pingIntervalMs=ms; try{ localStorage.setItem('diag_ping_interval_ms', String(pingIntervalMs)); }catch{} }
    // Post to host (extend payload end for future support)
    post('settings:save:'+'|'+'|'+(isNaN(parallel)?'':parallel)+'|'+(isNaN(timeout)?'':timeout)+'|'+dark+'|'+defSubnet+'|'+llmnr+'|'+mdns+'|'+nbns+'|'+nbt+'|'+(isNaN(pingRetriesVal)?'':pingRetriesVal)+'|'+(isNaN(pingIntervalSecVal)?'':pingIntervalSecVal));
  }

  // Auto-save when any setting changes (guarded)
  if(setDarkMode)           setDarkMode          .addEventListener('change',()=>{ applyDarkMode(setDarkMode.checked); saveSettings(); });
  if(setDiscoveryParallel)  setDiscoveryParallel .addEventListener('input', ()=>{ saveSettings(); });
  if(setDiscoveryTimeout)   setDiscoveryTimeout  .addEventListener('input', ()=>{ saveSettings(); });
  if(setEnableLlmnr)        setEnableLlmnr       .addEventListener('change',()=>{ saveSettings(); });
  if(setEnableMdns)         setEnableMdns        .addEventListener('change',()=>{ saveSettings(); });
  if(setEnableNbns)         setEnableNbns        .addEventListener('change',()=>{ saveSettings(); });
  if(setEnableNbtstat)      setEnableNbtstat     .addEventListener('change',()=>{ saveSettings(); });
  if(setPingRetriesEl)      setPingRetriesEl     .addEventListener('input', ()=>{ saveSettings(); });
  if(setPingIntervalEl)     setPingIntervalEl    .addEventListener('input', ()=>{ saveSettings(); });

  // Host -> UI: apply settings snapshot without triggering auto-save
  if(window.chrome && window.chrome.webview) window.chrome.webview.addEventListener('message',e=>{
    const msg=e.data; if(typeof msg!=='string') return;
    if(msg.startsWith('settings:data:')){
      try{
        isApplyingSettings=true;
        const s=JSON.parse(msg.substring('settings:data:'.length));
        if(setDiscoveryParallel) setDiscoveryParallel.value = s.DiscoveryParallel ?? '';
        if(setDiscoveryTimeout)  setDiscoveryTimeout .value = s.DiscoveryTimeout  ?? '';
        if(setDarkMode){ setDarkMode.checked = !!s.DarkMode; applyDarkMode(setDarkMode.checked); }
        if(setEnableLlmnr  && typeof s.EnableLlmnr  ==='boolean') setEnableLlmnr .checked=s.EnableLlmnr;
        if(setEnableMdns   && typeof s.EnableMdns   ==='boolean') setEnableMdns  .checked=s.EnableMdns;
        if(setEnableNbns   && typeof s.EnableNbns   ==='boolean') setEnableNbns  .checked=s.EnableNbns;
        if(setEnableNbtstat&& typeof s.EnableNbtstat==='boolean') setEnableNbtstat.checked=s.EnableNbtstat;
        // Diagnostics settings defaults/apply
        const storedRetries = (function(){ try{ return localStorage.getItem('diag_ping_retries'); }catch{ return null; } })();
        const storedInterval = (function(){ try{ return localStorage.getItem('diag_ping_interval_ms'); }catch{ return null; } })();
        if(setPingRetriesEl) setPingRetriesEl.value = storedRetries ?? (s.PingRetries ?? '0');
        if(setPingIntervalEl) setPingIntervalEl.value = storedInterval ? Math.max(1, Math.round(parseInt(storedInterval,10)/1000)).toString() : (s.PingIntervalSeconds ?? '2');
        // apply to runtime
        pingRetries = parseInt(setPingRetriesEl.value||'0',10) || 0;
        pingIntervalMs = (parseInt(setPingIntervalEl.value||'2',10) || 2) * 1000;
      }catch{}
      finally{ isApplyingSettings=false; }
    }
  });

  // expose subnet helper if needed
  window.getDefaultSubnetFromOctets = function(){
    const a=document.getElementById('defSub1')?.value||'';
    const b=document.getElementById('defSub2')?.value||'';
    const c=document.getElementById('defSub3')?.value||'';
    const d=document.getElementById('defSub4')?.value||'';
    return (a&&b&&c&&d)? `${a}.${b}.${c}.${d}` : '';
  };

  // =============================================
  // Table Sorting (Adapters + Discovery Results)
  // =============================================
  function makeTableSortable(tableId){
    const table=document.getElementById(tableId); if(!table) return;
    const ths=table.querySelectorAll('thead th');
    ths.forEach((th,idx)=>{
      th.addEventListener('click',()=>{
        const current=th.classList.contains('sort-asc')?'asc':th.classList.contains('sort-desc')?'desc':null;
        ths.forEach(h=>h.classList.remove('sort-asc','sort-desc'));
        const dir=current==='asc'?'desc':'asc';
        th.classList.add(dir==='asc'?'sort-asc':'sort-desc');
        sortTable(table,idx,dir==='asc');
      });
    });
  }
  function sortTable(table,colIndex,asc){
    const tbody=table.querySelector('tbody'); if(!tbody) return;
    const rows=[...tbody.querySelectorAll('tr')];
    rows.sort((a,b)=>{
      const av=getCellText(a,colIndex); const bv=getCellText(b,colIndex);
      const an=Number(av); const bn=Number(bv);
      const bothNum=!isNaN(an)&&!isNaN(bn);
      if(bothNum) return asc?an-bn:bn-an;
      return asc? av.localeCompare(bv,undefined,{numeric:true,sensitivity:'base'})
                : bv.localeCompare(av,undefined,{numeric:true,sensitivity:'base'});
    });
    rows.forEach(r=>tbody.appendChild(r));
  }
  function getCellText(row,idx){ const cell=row.children[idx]; return cell?cell.textContent.trim():''; }
  makeTableSortable('tblAdapters');
  makeTableSortable('discResults');

  // =============================================
  // Adapter selection sync
  // =============================================
  if(selAdapter) selAdapter.addEventListener('change',()=>{
    const selected=selAdapter.value; if(!selected) return;
    try{localStorage.setItem('ui_last_adapter',selected);}catch{}
    if(discAdapter){ const has=[...discAdapter.options].some(o=>o.value===selected); if(has){ discAdapter.value=selected; autoFillRangeFromSelected(); } }
    post('ui:selectedAdapter:'+selected);
  });
  try{ const lastAdapter=localStorage.getItem('ui_last_adapter'); if(lastAdapter && selAdapter){ selAdapter.value=lastAdapter; }}catch{}

  // =============================================
  // Initial data requests
  // =============================================
  post('adapters:request');
  post('favorites:request');
})();