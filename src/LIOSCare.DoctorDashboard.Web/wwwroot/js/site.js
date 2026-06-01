(function(){
  document.querySelectorAll('[data-sidebar-toggle]').forEach(btn=>btn.addEventListener('click',()=>document.getElementById('sidebar')?.classList.toggle('is-open')));
  const path = window.location.pathname.toLowerCase();
  document.querySelectorAll('.lc-nav a').forEach(a=>{const href=(a.getAttribute('href')||'').toLowerCase(); if(href && path.startsWith(href)) a.classList.add('active');});
  document.querySelectorAll('[data-toast]').forEach(t=>setTimeout(()=>{t.style.opacity='0';t.style.transform='translateY(-8px)';},4500));
  document.querySelectorAll('input[type="range"][data-range-output]').forEach(input=>{
    const target=document.getElementById(input.dataset.rangeOutput); const sync=()=>{if(target)target.textContent=input.value}; input.addEventListener('input',sync); sync();
  });
  setInterval(()=>document.querySelectorAll('[data-sla-deadline]').forEach(el=>{}),60000);
})();
