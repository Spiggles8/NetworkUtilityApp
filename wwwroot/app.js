(function(){
  const refreshBtn=document.getElementById('btnRefreshAdapters');
  const adaptersBody=document.querySelector('#tblAdapters tbody');
  const selAdapter=document.getElementById('selAdapter');
  const btnDhcp=document.getElementById('btnDhcp');
  const btnStatic=document.getElementById('btnStatic');
  const ipAddress=document.getElementById('ipAddress');
  const subnetMask=document.getElementById('subnetMask');
  const gateway=document.getElementById('gateway');
  const favoriteSelect=document.getElementById('favoriteSelect');
  let refreshing=false; let firstLoad=true;

  refreshBtn?.addEventListener('click',()=>{ if(refreshing) return; refreshing=true; adaptersBody.innerHTML='<tr class="loading"><td colspan="8">Refreshing...</td></tr>'; window.chrome?.webview?.postMessage('adapters:request'); });

  btnDhcp?.addEventListener('click',()=>{ const adapter=selAdapter.value; if(!adapter) return; window.chrome?.webview?.postMessage('adapters:setDhcp:'+adapter); });
  btnStatic?.addEventListener('click',()=>{ const adapter=selAdapter.value; if(!adapter) return; const ip=ipAddress.value.trim(); const mask=subnetMask.value.trim(); const gw=gateway.value.trim(); if(!ip||!mask) return; window.chrome?.webview?.postMessage('adapters:setStatic:'+adapter+'|'+ip+'|'+mask+'|'+gw); });

  favoriteSelect?.addEventListener('change',()=>{
    const opt=favoriteSelect.selectedOptions[0]; if(!opt || !opt.value) return;
    ipAddress.value=opt.value;
    subnetMask.value=opt.dataset.subnet || '';
    gateway.value=opt.dataset.gateway || deriveGateway(opt.value);
  });

  window.chrome?.webview?.addEventListener('message',e=>{
    const msg=e.data; if(typeof msg!=='string') return;
    if(msg.startsWith('adapters:data:')){
      const json=msg.substring('adapters:data:'.length);
      try{ const list=JSON.parse(json); const current=selAdapter.value; const frag=document.createDocumentFragment();
        if(!Array.isArray(list)||list.length===0){const tr=document.createElement('tr'); tr.innerHTML='<td colspan="8">No adapters found.</td>'; frag.appendChild(tr);} else {
          list.forEach(a=>{const tr=document.createElement('tr'); tr.innerHTML='<td>'+esc(a.AdapterName)+'</td><td>'+esc(a.IsDhcp)+'</td><td>'+esc(a.IpAddress)+'</td><td>'+esc(a.Subnet)+'</td><td>'+esc(a.Gateway)+'</td><td>'+esc(a.Status)+'</td><td>'+esc(a.HardwareDetails)+'</td><td>'+esc(a.MacAddress)+'</td>'; tr.addEventListener('click',()=>{[...adaptersBody.querySelectorAll('tr')].forEach(r=>r.classList.remove('selected')); tr.classList.add('selected'); selAdapter.value=a.AdapterName;}); if(a.AdapterName===current) tr.classList.add('selected'); frag.appendChild(tr); }); }
        adaptersBody.innerHTML=''; adaptersBody.appendChild(frag);
        if(firstLoad || !Array.isArray(list) || !list.some(a=>a.AdapterName===current)){ selAdapter.innerHTML='<option value="">(select)</option>'; if(Array.isArray(list)) list.forEach(a=> selAdapter.insertAdjacentHTML('beforeend','<option value="'+esc(a.AdapterName)+'">'+esc(a.AdapterName)+' ('+esc(a.IpAddress)+')</option>')); if(Array.isArray(list)&&list.some(a=>a.AdapterName===current)) selAdapter.value=current; }
        firstLoad=false;
      }catch{ adaptersBody.innerHTML='<tr><td colspan="8">Adapter JSON parse error.</td></tr>'; }
      finally { refreshing=false; }
    } else if(msg.startsWith('favorites:data:')){
      const json=msg.substring('favorites:data:'.length);
      try{ const favs=JSON.parse(json); favoriteSelect.innerHTML='<option value="">(none)</option>'; favs.forEach(f=>{ if(f.ip){ favoriteSelect.insertAdjacentHTML('beforeend','<option value="'+esc(f.ip)+'" data-subnet="'+esc(f.subnet)+'" data-gateway="'+(f.gateway?esc(f.gateway):'')+'">'+esc(f.ip)+'</option>'); } }); }catch{}
    }
  });

  function deriveGateway(ip){const p=ip.split('.'); return p.length===4? p[0]+'.'+p[1]+'.'+p[2]+'.1':'';}
  function esc(s){return (''+s).replace(/[&<>]/g,c=>({'&':'&amp;','<':'&lt;','>':'&gt;'}[c]));}
  refreshBtn?.click();
  window.chrome?.webview?.postMessage('favorites:request');
})();
