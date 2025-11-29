(function(){
  // Hides the preloader overlay after ~2 seconds
  function hidePreloader(){
    var overlay = document.getElementById('preloader-overlay');
    if(!overlay) return;
    // Add class that fades opacity to 0 (CSS transition is inlined in App.razor)
    overlay.classList.add('hidden');
    // Remove from DOM after the transition completes
    setTimeout(function(){ if(overlay.parentNode) overlay.parentNode.removeChild(overlay); }, 400);
  }

  // Theme helpers
  function applyTheme(theme){
    if(!theme){
      // remove both attributes to allow CSS fallback to prefers-color-scheme
      document.documentElement.removeAttribute('data-theme');
      document.documentElement.removeAttribute('data-bs-theme');
      return;
    }
    // keep both attributes in sync so Bootstrap and legacy CSS see the same theme
    document.documentElement.setAttribute('data-theme', theme);
    document.documentElement.setAttribute('data-bs-theme', theme);
  }

  function detectAndApplyTheme(){
    // Check saved preference
    try{
      // prefer tvc-theme (newer) but fall back to app-theme for backwards compatibility
      var saved = null;
      try {
        saved = localStorage.getItem('tvc-theme') || localStorage.getItem('app-theme');
      } catch (storageError) {
        console.warn('Theme: localStorage blocked by tracking prevention, using system preference');
      }
      if(saved === 'light' || saved === 'dark'){
        applyTheme(saved);
        return;
      }
    }catch(e){/*ignore*/}

    // Fall back to system preference
    var prefersDark = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
    applyTheme(prefersDark ? 'dark' : 'light');
  }

  // Expose controls to allow Blazor to toggle theme
  window.setAppTheme = function(theme){
    try{ localStorage.setItem('app-theme', theme); }catch(e){ console.warn('Could not save theme preference:', e.message); }
    try{ localStorage.setItem('tvc-theme', theme); }catch(e){}
    applyTheme(theme);
  };

  window.clearAppTheme = function(){
    try{ localStorage.removeItem('app-theme'); }catch(e){ console.warn('Could not clear theme preference:', e.message); }
    try{ localStorage.removeItem('tvc-theme'); }catch(e){}
    applyTheme(null);
  };

  // Ensure the overlay is visible for at least 2s after DOMContentLoaded
  document.addEventListener('DOMContentLoaded', function(){
    detectAndApplyTheme();
    setTimeout(hidePreloader, 2000);
  });

  // Safety fallback: hide after 5s even if DOMContentLoaded didn't fire
  setTimeout(hidePreloader, 5000);
})();
