(function(){
  // Safe post helper (avoids optional chaining warnings)
  function post(msg){try{if(window.chrome && window.chrome.webview){window.chrome.webview.postMessage(msg);}}catch{}}

  // Tab navigation
  const tabs=document.querySelectorAll('nav .tab-btn');
  const views=document.querySelectorAll('.view');
  tabs.forEach(btn=>btn.addEventListener('click',()=>{tabs.forEach(b=>b.classList.remove('active'));btn.classList.add('active');views.forEach(v=>v.classList.remove('active'));const t=document.getElementById('view-'+btn.dataset.view);if(t) t.classList.add('active');}));

  // Adapters
  const refreshBtn=document.getElementById('btnRefreshAdapters');
  const adaptersBody=document.querySelector('#tblAdapters tbody');
  const selAdapter=document.getElementById('selAdapter');
  const btnDhcp=document.getElementById('btnDhcp');
  const btnStatic=document.getElementById('btnStatic');
  const ipAddress=document.getElementById('ipAddress');
  const subnetMask=document.getElementById('subnetMask');
  const gateway=document.getElementById('gateway');
  const favoriteSelect=document.getElementById('favoriteSelect');
  let refreshing=false;let firstLoad=true;
  if(refreshBtn) refreshBtn.addEventListener('click',()=>{if(refreshing) return;refreshing=true;adaptersBody.innerHTML='<tr class="loading"><td colspan="8">Refreshing...</td></tr>';post('adapters:request');});
  if(btnDhcp) btnDhcp.addEventListener('click',()=>{const adapter=selAdapter.value;if(!adapter) return;post('adapters:setDhcp:'+adapter);});
  if(btnStatic) btnStatic.addEventListener('click',()=>{const adapter=selAdapter.value;if(!adapter) return;const ip=ipAddress.value.trim();const mask=subnetMask.value.trim();const gw=gateway.value.trim();if(!ip||!mask) return;post('adapters:setStatic:'+adapter+'|'+ip+'|'+mask+'|'+gw);});
  if(favoriteSelect) favoriteSelect.addEventListener('change',()=>{const opt=favoriteSelect.selectedOptions[0];if(!opt||!opt.value) return;ipAddress.value=opt.value;subnetMask.value=opt.dataset.subnet||'';gateway.value=opt.dataset.gateway||deriveGateway(opt.value);});

  // Diagnostics
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

  try{
    const savedPingCont=localStorage.getItem('diag_ping_continuous');
    if(savedPingCont==='true' && chkPingContinuous) chkPingContinuous.checked=true;
    const savedResolve=localStorage.getItem('diag_trace_resolve');
    if(savedResolve==='false' && chkTraceResolve) chkTraceResolve.checked=false;
  }catch{}

  if(chkPingContinuous) chkPingContinuous.addEventListener('change',()=>{try{localStorage.setItem('diag_ping_continuous', chkPingContinuous.checked? 'true':'false');}catch{}});
  if(chkTraceResolve) chkTraceResolve.addEventListener('change',()=>{try{localStorage.setItem('diag_trace_resolve', chkTraceResolve.checked? 'true':'false');}catch{}});

  let continuousPingTimer=null;const pingIntervalMs=2000;
  if(btnPing) btnPing.addEventListener('click',()=>{const t=pingTarget.value.trim();if(!t) return; if(chkPingContinuous && chkPingContinuous.checked){ if(continuousPingTimer){stopContinuousPing('[PING] Continuous stopped by user');return;} startContinuousPing(t); return;} doPing(t); });
  function startContinuousPing(t){btnPing.textContent='Stop';post('log:info:[PING] Continuous start: '+t);doPing(t);continuousPingTimer=setInterval(()=>doPing(t),pingIntervalMs);}
  function stopContinuousPing(reason){clearInterval(continuousPingTimer);continuousPingTimer=null;btnPing.textContent='Ping';post('log:info:'+reason);}
  function doPing(t){post('ping:'+t);}
  if(btnTrace) btnTrace.addEventListener('click',()=>{const t=traceTarget.value.trim();if(!t) return;post('log:info:[TRACE] Start '+t);post('trace:'+t+'|'+(chkTraceResolve && chkTraceResolve.checked?'resolve':'nresolve'));});
  if(btnNslookup) btnNslookup.addEventListener('click',()=>{const t=nsTarget.value.trim();if(!t) return;post('log:info:[NSLOOKUP] Start '+t);post('nslookup:'+t);});
  if(btnPathPing) btnPathPing.addEventListener('click',()=>{const t=pathPingTarget.value.trim();if(!t) return;post('log:info:[PATHPING] Start '+t);post('pathping:'+t);});
  if(btnDiagCancel) btnDiagCancel.addEventListener('click',()=>{ if(continuousPingTimer){ stopContinuousPing('[CANCEL] Continuous ping cancelled'); return;} post('diagnostics:cancel'); });

  if(window.chrome && window.chrome.webview) window.chrome.webview.addEventListener('message',e=>{const msg=e.data;if(typeof msg!=='string') return;if(msg.startsWith('adapters:data:')) handleAdapters(msg.substring('adapters:data:'.length)); else if(msg.startsWith('favorites:data:')) handleFavorites(msg.substring('favorites:data:'.length));});

  function handleAdapters(json){try{const list=JSON.parse(json);const current=selAdapter.value;const frag=document.createDocumentFragment();if(!Array.isArray(list)||list.length===0){const tr=document.createElement('tr');tr.innerHTML='<td colspan="8">No adapters found.</td>';frag.appendChild(tr);}else{list.forEach(a=>{const tr=document.createElement('tr');tr.innerHTML='<td>'+esc(a.AdapterName)+'</td><td>'+esc(a.IsDhcp)+'</td><td>'+esc(a.IpAddress)+'</td><td>'+esc(a.Subnet)+'</td><td>'+esc(a.Gateway)+'</td><td>'+esc(a.Status)+'</td><td>'+esc(a.HardwareDetails)+'</td><td>'+esc(a.MacAddress)+'</td>';tr.addEventListener('click',()=>{[...adaptersBody.querySelectorAll('tr')].forEach(r=>r.classList.remove('selected'));tr.classList.add('selected');selAdapter.value=a.AdapterName;});if(a.AdapterName===current) tr.classList.add('selected');frag.appendChild(tr);});}adaptersBody.innerHTML='';adaptersBody.appendChild(frag);if(firstLoad||!Array.isArray(list)||!list.some(a=>a.AdapterName===current)){selAdapter.innerHTML='<option value="">(select)</option>';if(Array.isArray(list)) list.forEach(a=> selAdapter.insertAdjacentHTML('beforeend','<option value="'+esc(a.AdapterName)+'">'+esc(a.AdapterName)+' ('+esc(a.IpAddress)+')</option>'));if(Array.isArray(list)&&list.some(a=>a.AdapterName===current)) selAdapter.value=current;}firstLoad=false;}catch{adaptersBody.innerHTML='<tr><td colspan="8">Adapter JSON parse error.</td></tr>';}finally{refreshing=false;}}
  function handleFavorites(json){try{const favs=JSON.parse(json);favoriteSelect.innerHTML='<option value="">(none)</option>';favs.forEach(f=>{if(f.ip){favoriteSelect.insertAdjacentHTML('beforeend','<option value="'+esc(f.ip)+'" data-subnet="'+esc(f.subnet)+'" data-gateway="'+(f.gateway?esc(f.gateway):'')+'">'+esc(f.ip)+'</option>');}});}catch{}}
  function deriveGateway(ip){const p=ip.split('.');return p.length===4? p[0]+'.'+p[1]+'.'+p[2]+'.1':'';}
  function esc(s){return(''+s).replace(/[&<>]/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;'}[c]));}

  // Discovery
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

  if(discAdapter) discAdapter.addEventListener('change',()=>autoFillRangeFromSelected());

  function autoFillRangeFromSelected(){
    const opt=discAdapter && discAdapter.selectedOptions[0];
    if(!opt) return;const ip=opt.getAttribute('data-ip');const subnet=opt.getAttribute('data-subnet');if(!ip||!subnet) return;
    const ipParts=ip.split('.').map(n=>parseInt(n,10));const maskParts=subnet.split('.').map(n=>parseInt(n,10));
    if(ipParts.length!==4||maskParts.length!==4||maskParts.some(isNaN)||ipParts.some(isNaN)) return;
    const networkAddr=[];const broadcastAddr=[];for(let i=0;i<4;i++){networkAddr[i]=ipParts[i]&maskParts[i];broadcastAddr[i]=networkAddr[i]|(255-maskParts[i]);}
    const firstUsable=[...networkAddr];const lastUsable=[...broadcastAddr];
    firstUsable[3]++;for(let i=3;i>0;i--){if(firstUsable[i]>255){firstUsable[i]=0;firstUsable[i-1]++;}}
    lastUsable[3]--;for(let i=3;i>0;i--){if(lastUsable[i]<0){lastUsable[i]=255;lastUsable[i-1]--;}}
    const isPointToPoint=subnet.trim()==='255.255.255.254';
    if(isPointToPoint){discStartIp.value=networkAddr.join('.');discEndIp.value=broadcastAddr.join('.');}
    else {discStartIp.value=firstUsable.join('.');discEndIp.value=lastUsable.join('.');}
  }

  if(discStart) discStart.addEventListener('click',()=>{discWasCancelled=false;const a=discAdapter.value.trim();const s=discStartIp.value.trim();const e=discEndIp.value.trim();if(!a||!s||!e){post('log:info:[DISC] Missing inputs.');return;}post('disc:start:'+a+'|'+s+'|'+e);});
  if(discCancel) discCancel.addEventListener('click',()=>{discWasCancelled=true;post('disc:cancel');});
  if(discSave) discSave.addEventListener('click',()=>{post('disc:save');});
  if(discClear) discClear.addEventListener('click',()=>{discWasCancelled=false;post('disc:reset');});
  function updateDiscStats(scanned,total,active,eta){if(discProgress)discProgress.textContent='Progress: '+scanned+' / '+total;if(discActive)discActive.textContent='Active: '+active;if(discEta)discEta.textContent='ETA: '+eta;}
  if(window.chrome && window.chrome.webview) window.chrome.webview.addEventListener('message',e=>{const msg=e.data;if(typeof msg!=='string')return;if(msg.startsWith('disc:adapters:')){const json=msg.substring('disc:adapters:'.length);try{const list=JSON.parse(json);discAdapter.innerHTML='';list.filter(a=>a.IpAddress).forEach(a=>{discAdapter.insertAdjacentHTML('beforeend','<option value="'+esc(a.AdapterName)+'" data-ip="'+esc(a.IpAddress)+'" data-subnet="'+esc(a.Subnet)+'">'+esc(a.AdapterName)+' ('+esc(a.IpAddress)+')</option>');});autoFillRangeFromSelected();}catch{}}else if(msg==='disc:cancelled'){discWasCancelled=true;}else if(msg.startsWith('disc:result:')){if(discWasCancelled)return;const json=msg.substring('disc:result:'.length);try{const r=JSON.parse(json);const tr=document.createElement('tr');tr.classList.add('active-row');tr.innerHTML='<td>'+esc(r.Ip)+'</td><td>'+esc(r.Hostname||'')+'</td><td>'+(r.LatencyMs??'')+'</td><td>'+esc(r.Mac||'')+'</td><td>'+esc(r.Manufacturer||'')+'</td><td>'+esc(r.Status||'')+'</td>';discResultsTbody.appendChild(tr);}catch{}}else if(msg.startsWith('disc:stats:')){const parts=msg.substring('disc:stats:'.length).split('|');if(parts.length>=4)updateDiscStats(parts[0],parts[1],parts[2],parts[3]);}else if(msg==='disc:clear'){discWasCancelled=false;discResultsTbody.innerHTML='';updateDiscStats(0,0,0,'--:--:--');}});

  // Settings tab
  const setDiscoveryParallel=document.getElementById('setDiscoveryParallel');
  const setDiscoveryTimeout=document.getElementById('setDiscoveryTimeout');
  const btnSettingsSave=document.getElementById('btnSettingsSave');
  const settingsStatus=document.getElementById('settingsStatus');
  const setDarkMode=document.getElementById('setDarkMode');

  function applyDarkMode(on){ document.body.classList[on? 'add':'remove']('dark'); }
  if(setDarkMode) setDarkMode.addEventListener('change',()=>{ applyDarkMode(setDarkMode.checked); post('settings:save:'+'|'+'|'+(setDiscoveryParallel.value||'')+'|'+(setDiscoveryTimeout.value||'')+'|'+(setDarkMode.checked? 'dark':'light')); });

  if(btnSettingsSave) btnSettingsSave.addEventListener('click',()=>{
    const parallel=parseInt(setDiscoveryParallel.value||'');
    const timeout=parseInt(setDiscoveryTimeout.value||'');
    post('settings:save:'+'|'+'|'+(isNaN(parallel)?'':parallel)+'|'+(isNaN(timeout)?'':timeout));
  });

  // Extend settings handler to read DarkMode
  if(window.chrome && window.chrome.webview) window.chrome.webview.addEventListener('message',e=>{ const msg=e.data; if(typeof msg!=='string') return; if(msg.startsWith('settings:data:')){ try{ const s=JSON.parse(msg.substring('settings:data:'.length)); if(setDiscoveryParallel && s.DiscoveryParallel) setDiscoveryParallel.value=s.DiscoveryParallel; if(setDiscoveryTimeout && s.DiscoveryTimeout) setDiscoveryTimeout.value=s.DiscoveryTimeout; if(setDarkMode) { setDarkMode.checked = s.DarkMode===true; applyDarkMode(setDarkMode.checked); } settingsStatus.textContent='Settings loaded.'; }catch{} } });

  post('settings:request');

  // Favorite IP preset management (Settings tab)
  const favSaveStatus=document.getElementById('favSaveStatus');
  const favSlot=document.getElementById('favSlot');
  const favIp=document.getElementById('favIp');
  const favSubnet=document.getElementById('favSubnet');
  const favGateway=document.getElementById('favGateway');
  const favSave=document.getElementById('favSave');

  if(favSave) favSave.addEventListener('click',()=>{
    const slot=parseInt(favSlot && favSlot.value ? favSlot.value : '1',10);
    const ip=favIp && favIp.value ? favIp.value.trim() : '';
    const subnet=favSubnet && favSubnet.value ? favSubnet.value.trim() : '';
    const gateway=favGateway && favGateway.value ? favGateway.value.trim() : '';
    if(!ip || !subnet){ favSaveStatus.textContent='IP and Subnet required.'; return; }
    post('favorites:save:'+slot+'|'+ip+'|'+subnet+'|'+gateway);
  });

  // Populate inputs from favorites when data arrives; also update when slot changes
  function populateFavInputsFromList(list){
    if(!Array.isArray(list)) return;
    const slot=parseInt(favSlot && favSlot.value ? favSlot.value : '1',10);
    const rec=list.find(f=>f.slot===slot);
    if(rec){ if(favIp && rec.ip) favIp.value=rec.ip; if(favSubnet && rec.subnet) favSubnet.value=rec.subnet; if(favGateway && rec.gateway) favGateway.value=rec.gateway; }
  }
  if(favSlot) favSlot.addEventListener('change',()=>{ /* ask for latest favorites then populate */ post('favorites:request'); });

  if(window.chrome && window.chrome.webview) window.chrome.webview.addEventListener('message',e=>{ const msg=e.data; if(typeof msg!=='string') return; if(msg.startsWith('favorites:data:')){ try{ const favs=JSON.parse(msg.substring('favorites:data:'.length)); populateFavInputsFromList(favs); }catch{} } else if(msg.startsWith('favorites:save:')){ favSaveStatus.textContent=msg.substring('favorites:save:'.length); post('favorites:request'); }});

  // Initial favorites load for settings tab
  post('favorites:request');

  // Sorting for HTML tables
  function makeTableSortable(tableId){const table=document.getElementById(tableId);if(!table)return;const ths=table.querySelectorAll('thead th');ths.forEach((th,idx)=>{th.addEventListener('click',()=>{const current=th.classList.contains('sort-asc')?'asc':th.classList.contains('sort-desc')?'desc':null;ths.forEach(h=>h.classList.remove('sort-asc','sort-desc'));const dir=current==='asc'?'desc':'asc';th.classList.add(dir==='asc'?'sort-asc':'sort-desc');sortTable(table,idx,dir==='asc');});});}
  function sortTable(table,colIndex,asc){const tbody=table.querySelector('tbody');if(!tbody)return;const rows=[...tbody.querySelectorAll('tr')];rows.sort((a,b)=>{const av=getCellText(a,colIndex);const bv=getCellText(b,colIndex);const an=Number(av);const bn=Number(bv);const bothNum=!isNaN(an)&&!isNaN(bn);if(bothNum)return asc?an-bn:bn-an;return asc?av.localeCompare(bv,undefined,{numeric:true,sensitivity:'base'}):bv.localeCompare(av,undefined,{numeric:true,sensitivity:'base'});});rows.forEach(r=>tbody.appendChild(r));}
  function getCellText(row,idx){const cell=row.children[idx];return cell?cell.textContent.trim():'';}
  makeTableSortable('tblAdapters');
  makeTableSortable('discResults');

  // Initial data requests
  post('adapters:request');
  post('disc:adapters');
  post('favorites:request');

})();