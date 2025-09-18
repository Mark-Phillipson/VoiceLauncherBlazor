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
      document.documentElement.removeAttribute('data-theme');
      return;
    }
    document.documentElement.setAttribute('data-theme', theme);
  }

  function detectAndApplyTheme(){
    // Check saved preference
    try{
      var saved = localStorage.getItem('app-theme');
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
    try{ localStorage.setItem('app-theme', theme); }catch(e){}
    applyTheme(theme);
  };

  window.clearAppTheme = function(){
    try{ localStorage.removeItem('app-theme'); }catch(e){}
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
