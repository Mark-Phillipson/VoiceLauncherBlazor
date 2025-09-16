// Lightweight custom scrollbar for the ListSidePanel
// Creates a visible, large, high-contrast scrollbar overlay that syncs with the native scroll position.
(function () {
  function ensureStyle() {
    if (document.getElementById('listpanel-scrollbar-style')) return;
    const css = `
      .lv-scrollbar-track {
        position: absolute;
        top: 0;
        right: 0;
        width: 22px;
        height: 100%;
        display: flex;
        align-items: flex-start;
        justify-content: center;
        pointer-events: auto; /* allow clicks on track to jump */
        background: transparent;
      }
      .lv-scrollbar-thumb {
        pointer-events: auto;
        width: 14px;
        margin-top: 4px;
        background: rgba(255,255,255,0.92);
        border-radius: 10px;
        box-shadow: 0 0 0 3px rgba(0,0,0,0.06) inset;
        transition: background 120ms linear, opacity 120ms linear;
        cursor: pointer;
        opacity: 0.98;
      }
      .lv-scrollbar-thumb:active { cursor: grabbing; }
      /* light mode contrast */
      @media (prefers-color-scheme: light) {
        .lv-scrollbar-thumb { background: rgba(0,0,0,0.45); box-shadow: 0 0 0 3px rgba(255,255,255,0.85) inset; }
      }
      .list-side-panel .lv-scrollbar-track,
      .list-side-panel .lv-scrollbar-thumb {
        z-index: 9999; /* keep on top of other UI chrome */
      }
    `;
    const s = document.createElement('style');
    s.id = 'listpanel-scrollbar-style';
    s.textContent = css;
    document.head.appendChild(s);
  }

  function clamp(v, a, b) { return Math.max(a, Math.min(b, v)); }

  function createFor(panelSelector) {
    const panel = document.querySelector(panelSelector);
    if (!panel) return null;
  // prefer explicit scroll target inside the panel (table wrapper), otherwise fall back to panel-body
  const panelBody = panel.querySelector('.table-scroll') || panel.querySelector('.panel-body');
    if (!panelBody) return null;

    // avoid double-init
    if (panel.__lv_scroll_inited) return panel.__lv_scroll_inited;

    // create track + thumb
    const track = document.createElement('div');
    track.className = 'lv-scrollbar-track';
    const thumb = document.createElement('div');
    thumb.className = 'lv-scrollbar-thumb';
    track.appendChild(thumb);

  // attach track to the scrolling body so it maps to the actual list area (not the header)
  panelBody.style.position = panelBody.style.position || 'relative';
  track.style.position = 'absolute';
  track.style.top = '0';
  track.style.right = '0';
  track.style.height = '100%';
  // initially hide track until panel is visible and scrollable
  track.style.display = 'none';
  panelBody.appendChild(track);

    // make sure track can receive pointer events
    track.style.pointerEvents = 'auto';

    // when the panel completes its open/close transition, try updating sizes
    panel.addEventListener('transitionend', (ev) => {
      try { update(); }
      catch (e) {}
    });

    // observe attribute changes (style/class) in case framework toggles classes instead of transform
    const mo = new MutationObserver(() => {
      try { update(); }
      catch (e) {}
    });
    mo.observe(panel, { attributes: true, attributeFilter: ['style', 'class'] });

    // compute sizes and sync
    function isPanelVisible() {
      try {
        const r = panel.getBoundingClientRect();
        const cs = getComputedStyle(panel);
        // visible and not translated off-screen
        if (cs.display === 'none' || cs.visibility === 'hidden') return false;
        // if left is negative the panel is translated off-canvas
        if (r.left < 0 && r.right <= 0) return false;
        return true;
      } catch (e) {
        return false;
      }
    }

    function update() {
      // If panel isn't visible yet, hide thumb and let the retry loop handle update when it opens
      if (!isPanelVisible()) {
        thumb.style.display = 'none';
        return;
      }
      const sh = panelBody.scrollHeight;
      const ch = panelBody.clientHeight;
      if (sh <= ch || ch === 0) {
        // hide track/thumb when not needed or height not initialized yet
        thumb.style.display = 'none';
        track.style.display = 'none';
        return;
      }
      track.style.display = '';
      thumb.style.display = '';
      const thumbH = clamp(Math.max((ch / sh) * ch, 28), 28, Math.max(28, ch - 8));
      thumb.style.height = thumbH + 'px';
      const maxThumbTop = ch - thumbH - 8; // padding
      const scrollRatio = panelBody.scrollTop / (sh - ch || 1);
      const top = Math.round(scrollRatio * maxThumbTop) + 4;
      thumb.style.marginTop = top + 'px';
    }

    // handle dragging
    let dragging = false;
    let dragStartY = 0;
    let startScrollTop = 0;

    thumb.addEventListener('pointerdown', (e) => {
      e.preventDefault();
      thumb.setPointerCapture(e.pointerId);
      dragging = true;
      dragStartY = e.clientY;
      startScrollTop = panelBody.scrollTop;
    });

    window.addEventListener('pointermove', (e) => {
      if (!dragging) return;
      const dy = e.clientY - dragStartY;
      const sh = panelBody.scrollHeight;
      const ch = panelBody.clientHeight;
      const thumbH = thumb.offsetHeight;
      const maxThumbTop = ch - thumbH - 8;
      const ratio = (sh - ch) / (maxThumbTop || 1);
      panelBody.scrollTop = clamp(startScrollTop + dy * ratio, 0, sh - ch);
    });

    window.addEventListener('pointerup', (e) => {
      if (dragging) {
        dragging = false;
      }
    });

    // click on track to jump (maps to panelBody scroll)
    track.addEventListener('click', (e) => {
      if (e.target === thumb) return; // ignore thumb clicks
      // Use track rect so clicks map to the visible track area (robust if track is inside panelBody)
      const trackRect = track.getBoundingClientRect();
      const clickY = e.clientY - trackRect.top;
      const sh = panelBody.scrollHeight;
      const ch = panelBody.clientHeight;
      const thumbH = thumb.offsetHeight || 28;
      const maxThumbTop = Math.max(trackRect.height - thumbH - 8, 0);
      const ratio = (sh - ch) / (maxThumbTop || 1);
      const newThumbTop = clamp(clickY - thumbH / 2, 4, maxThumbTop + 4);
      panelBody.scrollTop = Math.round((newThumbTop - 4) * ratio);
    });

    // sync on scroll and on resize
    panelBody.addEventListener('scroll', update);
    new ResizeObserver(update).observe(panelBody);

  // initial update (may hide until panel becomes visible)
  setTimeout(update, 40);

    // Retry repeatedly for a short while to handle Blazor/transition timing where the panel opens after init.
    // Keep retrying until panel is visible and sizes are stabilised, up to ~2.5s.
    let retries = 0;
    const retryHandle = setInterval(() => {
      try { update(); }
      catch (e) {}
      retries++;
      // stop retries when panel is visible and has overflow or when we've retried enough
      const sh = panelBody.scrollHeight;
      const ch = panelBody.clientHeight;
      if (isPanelVisible() && sh > ch && ch > 0) {
        clearInterval(retryHandle);
      }
      if (retries > 25) clearInterval(retryHandle); // ~25 * 100ms = 2.5s of retries
    }, 100);

    panel.__lv_scroll_inited = { panel, panelBody, track, thumb, update };
    return panel.__lv_scroll_inited;
  }

  window.ListSidePanelScrollbar = {
    init(selector) {
      try {
        ensureStyle();
        return createFor(selector);
      } catch (e) {
        console.error('ListSidePanelScrollbar.init error', e);
      }
      return null;
    }
  };
})();
