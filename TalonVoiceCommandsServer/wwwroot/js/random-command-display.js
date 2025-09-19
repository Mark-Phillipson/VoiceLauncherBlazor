// Random Command Display for TalonVoiceCommandSearch
// Displays random voice commands when the form is idle for user discovery

window.RandomCommandDisplay = {
    // Configuration
    idleTimeMs: 2 * 60 * 1000, // 2 minutes of inactivity
    displayIntervalMs: 5000, // Show each command for 5 seconds
    commandCount: 6, // Number of random commands to cycle through
    
    // State
    isActive: false,
    isIdle: false,
    currentCommands: [],
    currentIndex: 0,
    lastActivityTime: Date.now(),
    idleTimer: null,
    displayTimer: null,
    
    // DOM elements (will be initialized on start)
    container: null,
    
    // Initialize the random command display system
    init() {
        console.log('RandomCommandDisplay: Initializing...');
        
        // Create the display container
        this.createDisplayContainer();
        
        // Set up activity tracking
        this.setupActivityTracking();
        
        // Start the idle monitoring
        this.startIdleMonitoring();
        
        console.log('RandomCommandDisplay: Initialized successfully');
    },
    
    // Create the HTML container for displaying random commands
    createDisplayContainer() {
        // Check if container already exists
        if (document.getElementById('random-command-display')) {
            this.container = document.getElementById('random-command-display');
            return;
        }
        
        // Create the container element
        this.container = document.createElement('div');
        this.container.id = 'random-command-display';
        this.container.className = 'random-command-display';
        this.container.style.display = 'none';
        
        // Add the HTML structure
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
        
        // Insert the container into the search results area
        const searchResults = document.querySelector('.search-results-container');
        if (searchResults) {
            searchResults.parentNode.insertBefore(this.container, searchResults);
        } else {
            // Fallback: insert after the search form
            const searchForm = document.querySelector('form');
            if (searchForm) {
                searchForm.parentNode.insertBefore(this.container, searchForm.nextSibling);
            }
        }
    },
    
    // Set up activity tracking to detect user interaction
    setupActivityTracking() {
        const events = ['mousedown', 'mousemove', 'keypress', 'scroll', 'touchstart', 'click', 'focus', 'input'];
        
        // Reset activity timer on any user interaction
        const resetActivity = () => {
            this.lastActivityTime = Date.now();
            
            // If we were showing random commands, hide them
            if (this.isActive) {
                this.hide();
            }
        };
        
        // Add event listeners
        events.forEach(event => {
            document.addEventListener(event, resetActivity, { passive: true });
        });
        
        console.log('RandomCommandDisplay: Activity tracking set up');
    },
    
    // Start monitoring for idle state
    startIdleMonitoring() {
        this.idleTimer = setInterval(() => {
            const timeSinceActivity = Date.now() - this.lastActivityTime;
            const shouldBeIdle = timeSinceActivity >= this.idleTimeMs;
            
            if (shouldBeIdle && !this.isIdle && this.canShowRandomCommands()) {
                console.log('RandomCommandDisplay: User is idle, starting random command display');
                this.startDisplay();
            }
            
            this.isIdle = shouldBeIdle;
        }, 10000); // Check every 10 seconds
    },
    
    // Check if conditions are met to show random commands
    canShowRandomCommands() {
        // Check if search box is empty
        const searchInput = document.querySelector('#searchTerm, input[type="text"]');
        if (searchInput && searchInput.value.trim() !== '') {
            return false;
        }
        
        // Check if any filters are active by looking at the button text content
        const filterButtons = document.querySelectorAll('.filter-btn-application, .filter-btn-mode, .filter-btn-tags, .filter-btn-os, .filter-btn-repository, .filter-btn-title, .filter-btn-code-language');
        for (const button of filterButtons) {
            const text = button.textContent.trim();
            // Skip empty buttons or buttons that contain default "All " text
            if (text === '' || text.includes('All Applications') || text.includes('All Modes') || 
                text.includes('All Tags') || text.includes('All Operating Systems') || 
                text.includes('All Repositories') || text.includes('All Titles') || 
                text.includes('All Code Languages')) {
                continue; // This is a default state
            }
            // If we get here, there's an active filter
            return false;
        }
        
        // Check if auto-filter by app is enabled
        const autoFilterCheckbox = document.querySelector('#autoFilterToggle');
        if (autoFilterCheckbox && autoFilterCheckbox.checked) {
            return false;
        }
        
        return true;
    },
    
    // Start displaying random commands
    async startDisplay() {
        try {
            this.isActive = true;
            
            // Get random commands from IndexedDB
            console.log('RandomCommandDisplay: Fetching random commands...');
            this.currentCommands = await window.TalonStorageDB.getRandomCommands(this.commandCount);
            
            if (this.currentCommands.length === 0) {
                console.log('RandomCommandDisplay: No commands available');
                return;
            }
            
            console.log(`RandomCommandDisplay: Got ${this.currentCommands.length} random commands`);
            
            // Reset to first command
            this.currentIndex = 0;
            
            // Show the container and first command
            this.show();
            this.displayCurrentCommand();
            
            // Start the rotation timer
            this.startRotation();
            
        } catch (error) {
            console.error('RandomCommandDisplay: Error starting display:', error);
            this.isActive = false;
        }
    },
    
    // Start automatic rotation through commands
    startRotation() {
        if (this.displayTimer) {
            clearInterval(this.displayTimer);
        }
        
        this.displayTimer = setInterval(() => {
            if (this.isActive) {
                this.nextCommand();
            }
        }, this.displayIntervalMs);
    },
    
    // Show the display container
    show() {
        if (this.container) {
            this.container.style.display = 'block';
            // Fade in animation
            setTimeout(() => {
                this.container.style.opacity = '1';
            }, 10);
        }
    },
    
    // Hide the display container
    hide() {
        if (this.container) {
            this.container.style.opacity = '0';
            setTimeout(() => {
                this.container.style.display = 'none';
            }, 300);
        }
        
        this.isActive = false;
        
        if (this.displayTimer) {
            clearInterval(this.displayTimer);
            this.displayTimer = null;
        }
    },
    
    // Display the current command
    displayCurrentCommand() {
        if (!this.container || this.currentCommands.length === 0) return;
        
        const command = this.currentCommands[this.currentIndex];
        
        // Update command text
        const commandTextEl = this.container.querySelector('.command-text');
        if (commandTextEl) {
            commandTextEl.textContent = command.Command || command.command || 'Unknown command';
        }
        
        // Update application badge
        const appBadgeEl = this.container.querySelector('.application-badge');
        if (appBadgeEl) {
            const app = command.Application || command.application || 'Unknown';
            appBadgeEl.textContent = app;
            appBadgeEl.className = 'application-badge badge filter-btn-application';
        }
        
        // Update script preview
        const scriptPreviewEl = this.container.querySelector('.script-preview');
        if (scriptPreviewEl) {
            const script = command.Script || command.script || '';
            const preview = script.split('\n')[0] || ''; // First line only
            scriptPreviewEl.textContent = preview.length > 60 ? preview.substring(0, 60) + '...' : preview;
        }
        
        // Update progress indicator
        const currentIndexEl = this.container.querySelector('.current-index');
        const totalCountEl = this.container.querySelector('.total-count');
        if (currentIndexEl) currentIndexEl.textContent = (this.currentIndex + 1).toString();
        if (totalCountEl) totalCountEl.textContent = this.currentCommands.length.toString();
    },
    
    // Navigate to next command
    nextCommand() {
        if (this.currentCommands.length === 0) return;
        
        this.currentIndex = (this.currentIndex + 1) % this.currentCommands.length;
        this.displayCurrentCommand();
    },
    
    // Navigate to previous command
    previousCommand() {
        if (this.currentCommands.length === 0) return;
        
        this.currentIndex = this.currentIndex === 0 ? this.currentCommands.length - 1 : this.currentIndex - 1;
        this.displayCurrentCommand();
    },
    
    // Search for the current command
    searchForCommand() {
        if (this.currentCommands.length === 0) return;
        
        const command = this.currentCommands[this.currentIndex];
        const commandText = command.Command || command.command || '';
        
        // Fill the search box with the command text
        const searchInput = document.querySelector('#searchTerm, input[type="text"]');
        if (searchInput) {
            searchInput.value = commandText;
            
            // Trigger search by dispatching events
            searchInput.dispatchEvent(new Event('input', { bubbles: true }));
            searchInput.dispatchEvent(new Event('change', { bubbles: true }));
            
            // Focus the search input
            searchInput.focus();
        }
        
        // Hide the random display
        this.hide();
    },
    
    // Dismiss the random command display
    dismiss() {
        this.hide();
        
        // Reset activity timer so it doesn't show again immediately
        this.lastActivityTime = Date.now();
    },
    
    // Clean up timers and event listeners
    destroy() {
        if (this.idleTimer) {
            clearInterval(this.idleTimer);
            this.idleTimer = null;
        }
        
        if (this.displayTimer) {
            clearInterval(this.displayTimer);
            this.displayTimer = null;
        }
        
        this.isActive = false;
        this.isIdle = false;
        
        if (this.container && this.container.parentNode) {
            this.container.parentNode.removeChild(this.container);
            this.container = null;
        }
        
        console.log('RandomCommandDisplay: Destroyed');
    }
};

// Auto-initialize when DOM is ready
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', () => {
        window.RandomCommandDisplay.init();
    });
} else {
    // DOM is already ready
    window.RandomCommandDisplay.init();
}