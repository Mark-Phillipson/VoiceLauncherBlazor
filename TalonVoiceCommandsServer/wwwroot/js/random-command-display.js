// Random Command Display for TalonVoiceCommandSearch
// Clean self-contained module with debugCenter and testShow

window.RandomCommandDisplay = (function () {
    const module = {
        // Configuration
        idleTimeMs: 2 * 60 * 1000,
        displayIntervalMs: 5000,
        commandCount: 6,

    // State
        isActive: false,
        isIdle: false,
        currentCommands: [],
        currentIndex: 0,
        lastActivityTime: Date.now(),
        idleTimer: null,
        displayTimer: null,
        ignoreActivityUntil: 0,
    // When true, user activity will not auto-hide the overlay. Useful for debugging.
    debugDisableAutoHide: false,
        debugCenter: false,

        // DOM
        container: null,

        init() {
            console.log('RandomCommandDisplay: Initializing...');
            this.createDisplayContainer();
            this.setupActivityTracking();
            this.startIdleMonitoring();
            // Debug controls for quick manual testing (Show / Hide)
            try { this.createDebugControls(); } catch (e) { console.warn('RandomCommandDisplay: createDebugControls failed', e); }
            console.log('RandomCommandDisplay: Initialized successfully');
        },

        // Adds two small debug buttons to the page to call show() and hide() manually.
        createDebugControls() {
            if (document.getElementById('random-command-debug-controls')) return;
            const wrapper = document.createElement('div');
            wrapper.id = 'random-command-debug-controls';
            wrapper.style.position = 'fixed';
            wrapper.style.left = '1rem';
            wrapper.style.bottom = '1rem';
            wrapper.style.zIndex = '1060';
            wrapper.style.display = 'flex';
            wrapper.style.gap = '0.5rem';

            const btnShow = document.createElement('button');
            btnShow.type = 'button';
            btnShow.textContent = 'Show';
            btnShow.className = 'btn btn-sm btn-primary';
            btnShow.onclick = function () { try { window.RandomCommandDisplay && window.RandomCommandDisplay.show(); } catch (e) { console.warn(e); } };

            const btnHide = document.createElement('button');
            btnHide.type = 'button';
            btnHide.textContent = 'Hide';
            btnHide.className = 'btn btn-sm btn-secondary';
            btnHide.onclick = function () { try { window.RandomCommandDisplay && window.RandomCommandDisplay.hide(); } catch (e) { console.warn(e); } };

            wrapper.appendChild(btnShow);
            wrapper.appendChild(btnHide);

            try { document.body.appendChild(wrapper); }
            catch (err) { console.warn('RandomCommandDisplay: append debug controls failed', err); }
        },

        createDisplayContainer() {
            if (document.getElementById('random-command-display')) {
                this.container = document.getElementById('random-command-display');
                return;
            }
            this.container = document.createElement('div');
            this.container.id = 'random-command-display';
            this.container.className = 'random-command-display';
            // Hidden by default; will be shown by `show()` when ready.
            this.container.style.display = 'none';
            this.container.style.opacity = '0';
            this.container.style.position = 'fixed';
            this.container.style.right = '1rem';
            this.container.style.bottom = '1rem';
            this.container.style.zIndex = '1050';
            this.container.style.maxWidth = '420px';
            this.container.style.width = 'min(420px, 95vw)';
            // Visual styling is applied in show() when the overlay becomes active.
            this.container.style.transition = 'opacity 0.25s ease, transform 0.25s ease';

            this.container.innerHTML = `
                <div class="random-command-card">
                    <div class="random-command-header">
                        <h5><i class="oi oi-lightbulb"></i> Discover Voice Commands</h5>
                        <button type="button" class="btn-close" aria-label="Dismiss" onclick="RandomCommandDisplay.dismiss()"></button>
                    </div>
                    <div class="random-command-content">
                        <div class="command-text"></div>
                        <div class="command-details">
                            <span class="application-badge"></span>
                            <span class="script-preview"></span>
                        </div>
                    </div>
                    <div class="random-command-footer">
                        <div class="progress-indicator">
                            <span class="current-index">1</span> of <span class="total-count">6</span>
                        </div>
                        <div class="controls">
                            <button type="button" class="btn btn-sm btn-outline-secondary" onclick="RandomCommandDisplay.previousCommand()">
                                <i class="oi oi-chevron-left"></i>
                            </button>
                            <button type="button" class="btn btn-sm btn-outline-secondary" onclick="RandomCommandDisplay.nextCommand()">
                                <i class="oi oi-chevron-right"></i>
                            </button>
                            <button type="button" class="btn btn-sm btn-outline-primary" onclick="RandomCommandDisplay.searchForCommand()">
                                <i class="oi oi-magnifying-glass"></i> Search
                            </button>
                        </div>
                    </div>
                </div>
            `;

            try { document.body.appendChild(this.container); }
            catch (err) {
                console.warn('RandomCommandDisplay: append to body failed, fallback', err);
                const searchForm = document.querySelector('form');
                if (searchForm && searchForm.parentNode) searchForm.parentNode.insertBefore(this.container, searchForm.nextSibling);
                else document.body.appendChild(this.container);
            }
        },

        setupActivityTracking() {
            const events = ['mousedown', 'mousemove', 'keypress', 'scroll', 'touchstart', 'click', 'focus', 'input'];
            const resetActivity = (evt) => {
                try {
                    const now = Date.now();
                    const target = evt && evt.target ? evt.target : null;
                    // Debug logging
                    try { console.debug('RandomCommandDisplay: activity', evt && evt.type, target && (target.id || target.className || target.tagName)); } catch (e) { }
                    if (target) {
                        if (target.closest && target.closest('.start-random-demo-btn')) { this.ignoreActivityUntil = Date.now() + 800; this.lastActivityTime = Date.now(); console.debug('RandomCommandDisplay: ignored activity from start button'); return; }
                        if (target.closest && target.closest('#random-command-display')) { this.lastActivityTime = Date.now(); console.debug('RandomCommandDisplay: activity inside display — updating lastActivityTime'); return; }
                    }
                    if (now < this.ignoreActivityUntil) { this.lastActivityTime = Date.now(); console.debug('RandomCommandDisplay: within ignoreActivityUntil window'); return; }
                } catch (e) { console.warn('RandomCommandDisplay: resetActivity error', e); }
                this.lastActivityTime = Date.now();
                if (this.isActive) {
                    if (this.debugDisableAutoHide) { console.debug('RandomCommandDisplay: auto-hide suppressed (debugDisableAutoHide)'); }
                    else { console.debug('RandomCommandDisplay: auto-hiding due to user activity'); this.hide(); }
                }
            };
            events.forEach(event => document.addEventListener(event, resetActivity, { passive: true }));
            console.log('RandomCommandDisplay: Activity tracking set up');
        },

        startIdleMonitoring() {
            this.idleTimer = setInterval(() => {
                const timeSinceActivity = Date.now() - this.lastActivityTime;
                const shouldBeIdle = timeSinceActivity >= this.idleTimeMs;
                if (shouldBeIdle && !this.isIdle && this.canShowRandomCommands()) { console.log('RandomCommandDisplay: User is idle, starting random command display'); this.startDisplay(); }
                this.isIdle = shouldBeIdle;
            }, 10000);
        },

        canShowRandomCommands() {
            const searchInput = document.querySelector('#searchTerm, input[type="text"]'); if (searchInput && searchInput.value.trim() !== '') return false;
            const filterButtons = document.querySelectorAll('.filter-btn-application, .filter-btn-mode, .filter-btn-tags, .filter-btn-os, .filter-btn-repository, .filter-btn-title, .filter-btn-code-language');
            for (const button of filterButtons) { const text = button.textContent.trim(); if (text === '' || text.includes('All Applications') || text.includes('All Modes') || text.includes('All Tags') || text.includes('All Operating Systems') || text.includes('All Repositories') || text.includes('All Titles') || text.includes('All Code Languages')) continue; return false; }
            const autoFilterCheckbox = document.querySelector('#autoFilterToggle'); if (autoFilterCheckbox && autoFilterCheckbox.checked) return false; return true;
        },

        async startDisplay() {
            try {
                this.isActive = true;
                console.log('RandomCommandDisplay: Fetching random commands...');
                this.currentCommands = await window.TalonStorageDB.getRandomCommands(this.commandCount);
                if (!this.currentCommands || this.currentCommands.length === 0) { console.log('RandomCommandDisplay: No commands available'); this.isActive = false; return; }
                this.currentIndex = 0; this.show(); this.displayCurrentCommand(); this.startRotation();
            } catch (err) { console.error('RandomCommandDisplay: Error starting display', err); this.isActive = false; }
        },

        startRotation() { if (this.displayTimer) clearInterval(this.displayTimer); this.displayTimer = setInterval(() => { if (this.isActive) this.nextCommand(); }, this.displayIntervalMs); },

        show() {
            if (!this.container) { console.warn('RandomCommandDisplay: no container'); return; }
            this.container.style.display = 'block'; this.container.setAttribute('aria-hidden', 'false');
            if (this.debugCenter) {
                this.container.style.left = '50%'; this.container.style.top = '50%'; this.container.style.right = 'auto'; this.container.style.bottom = 'auto'; this.container.style.transform = 'translate(-50%, -50%)'; this.container.style.width = 'min(900px, 90vw)'; this.container.style.maxWidth = '900px'; this.container.style.backgroundColor = '#fff'; this.container.style.border = '4px solid #1976d2'; this.container.style.boxShadow = '0 20px 60px rgba(0,0,0,0.35)'; this.container.style.padding = '1.5rem'; this.container.style.borderRadius = '0.75rem'; this.container.style.color = '#000'; this.container.style.fontSize = '16px'; this.container.style.lineHeight = '1.4'; this.container.style.zIndex = '2147483647';
            } else {
                this.container.style.right = '1rem'; this.container.style.bottom = '1rem'; this.container.style.transform = 'none'; this.container.style.maxWidth = '420px'; this.container.style.backgroundColor = '#fff59d'; this.container.style.border = '3px solid #d32f2f'; this.container.style.boxShadow = '0 10px 30px rgba(0,0,0,0.25)'; this.container.style.padding = '1rem'; this.container.style.borderRadius = '0.5rem'; this.container.style.color = '#000'; this.container.style.fontSize = '14px'; this.container.style.lineHeight = '1.3'; this.container.style.zIndex = '99999';
            }

            setTimeout(() => { this.container.style.opacity = '1'; setTimeout(() => { try { const ev = new CustomEvent('random-command-display:shown'); this.container.dispatchEvent(ev); window.dispatchEvent(ev); } catch (e) { } this.debugCenter = false; }, 320); }, 10);
        },

        hide() {
            if (!this.container) return;
            try { console.debug('RandomCommandDisplay: hide() called — isActive=', this.isActive, 'lastActivityTime=', this.lastActivityTime); } catch (e) { }
            this.container.style.opacity = '0';
            setTimeout(() => {
                try { this.container.style.display = 'none'; } catch { }
                try { this.container.setAttribute('aria-hidden', 'true'); } catch (e) { }
            }, 300);
            this.isActive = false;
            if (this.displayTimer) { clearInterval(this.displayTimer); this.displayTimer = null; }
        },

        displayCurrentCommand() { if (!this.container || !this.currentCommands || this.currentCommands.length === 0) return; const command = this.currentCommands[this.currentIndex]; const commandTextEl = this.container.querySelector('.command-text'); if (commandTextEl) commandTextEl.textContent = command.Command || command.command || 'Unknown command'; const appBadgeEl = this.container.querySelector('.application-badge'); if (appBadgeEl) { const app = command.Application || command.application || 'Unknown'; appBadgeEl.textContent = app; appBadgeEl.className = 'application-badge badge filter-btn-application'; } const scriptPreviewEl = this.container.querySelector('.script-preview'); if (scriptPreviewEl) { const script = command.Script || command.script || ''; const preview = script.split('\n')[0] || ''; scriptPreviewEl.textContent = preview.length > 60 ? preview.substring(0, 60) + '...' : preview; } const currentIndexEl = this.container.querySelector('.current-index'); const totalCountEl = this.container.querySelector('.total-count'); if (currentIndexEl) currentIndexEl.textContent = (this.currentIndex + 1).toString(); if (totalCountEl) totalCountEl.textContent = this.currentCommands.length.toString(); },

        nextCommand() { if (!this.currentCommands || this.currentCommands.length === 0) return; this.currentIndex = (this.currentIndex + 1) % this.currentCommands.length; this.displayCurrentCommand(); },
        previousCommand() { if (!this.currentCommands || this.currentCommands.length === 0) return; this.currentIndex = this.currentIndex === 0 ? this.currentCommands.length - 1 : this.currentIndex - 1; this.displayCurrentCommand(); },

        searchForCommand() { if (!this.currentCommands || this.currentCommands.length === 0) return; const command = this.currentCommands[this.currentIndex]; const commandText = command.Command || command.command || ''; const searchInput = document.querySelector('#searchTerm, input[type="text"]'); if (searchInput) { searchInput.value = commandText; searchInput.dispatchEvent(new Event('input', { bubbles: true })); searchInput.dispatchEvent(new Event('change', { bubbles: true })); searchInput.focus(); } this.hide(); },

        dismiss() { this.hide(); this.lastActivityTime = Date.now(); },

        destroy() { if (this.idleTimer) { clearInterval(this.idleTimer); this.idleTimer = null; } if (this.displayTimer) { clearInterval(this.displayTimer); this.displayTimer = null; } this.isActive = false; this.isIdle = false; if (this.container && this.container.parentNode) { this.container.parentNode.removeChild(this.container); this.container = null; } console.log('RandomCommandDisplay: Destroyed'); },

        testShow(timeoutMs = 5000) { return new Promise((resolve, reject) => { let timedOut = false; const onShown = () => { if (timedOut) return; clearTimeout(timer); window.removeEventListener('random-command-display:shown', onShown); resolve(true); }; const timer = setTimeout(() => { timedOut = true; window.removeEventListener('random-command-display:shown', onShown); reject(new Error('RandomCommandDisplay.testShow: timeout waiting for overlay to appear')); }, timeoutMs); window.addEventListener('random-command-display:shown', onShown); this.startDisplay().catch(err => { clearTimeout(timer); window.removeEventListener('random-command-display:shown', onShown); reject(err); }); }); }
    };

    if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', () => module.init()); else module.init();

    return module;
})();

// Safe starter exposed globally so inline handlers can call without racing the module load.
// Usage: window.startRandomDemo({ debugCenter: true })
window.startRandomDemo = function (opts) {
    opts = opts || {};
    const start = () => {
        try {
            if (window.RandomCommandDisplay && typeof window.RandomCommandDisplay.startDisplay === 'function') {
                if (opts.debugCenter) window.RandomCommandDisplay.debugCenter = true;
                // Instruct the module to ignore user activity for a short window so the
                // initiating click doesn't cause the overlay to immediately hide.
                try { window.RandomCommandDisplay.ignoreActivityUntil = Date.now() + 4000; window.RandomCommandDisplay.lastActivityTime = Date.now(); } catch (e) { /* ignore */ }
                // small defer so any immediate click event doesn't count as activity
                setTimeout(() => { window.RandomCommandDisplay.startDisplay(); }, 120);
                return true;
            }
        } catch (e) { /* ignore */ }
        return false;
    };

    const maxAttempts = 40; // ~3.2s
    let attempts = 0;
    const tick = () => {
        if (start()) return;
        attempts += 1;
        if (attempts >= maxAttempts) {
            console.warn('startRandomDemo: RandomCommandDisplay not available after waiting');
            return;
        }
        setTimeout(tick, 80);
    };
    tick();
};

// Helper to toggle auto-hide behavior from console for debugging.
window.toggleRandomDemoAutoHide = function (enableAutoHide) {
    try {
        if (window.RandomCommandDisplay) {
            window.RandomCommandDisplay.debugDisableAutoHide = !enableAutoHide;
            console.log('toggleRandomDemoAutoHide: set debugDisableAutoHide =', window.RandomCommandDisplay.debugDisableAutoHide);
            return true;
        }
    } catch (e) { }
    console.warn('toggleRandomDemoAutoHide: RandomCommandDisplay not available');
    return false;
};
