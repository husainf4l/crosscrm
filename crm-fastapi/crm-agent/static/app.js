// Modern Chat Interface JavaScript
class ChatInterface {
    constructor() {
        this.apiBase = window.location.origin;
        this.userId = 1;
        this.isLoading = false;
        
        this.initializeElements();
        this.attachEventListeners();
        this.loadChatHistory();
    }

    initializeElements() {
        this.chatMessages = document.getElementById('chatMessages');
        this.messageInput = document.getElementById('messageInput');
        this.sendBtn = document.getElementById('sendBtn');
        this.chatForm = document.getElementById('chatForm');
        this.userIdInput = document.getElementById('userIdInput');
        this.loadHistoryBtn = document.getElementById('loadHistoryBtn');
        this.clearChatBtn = document.getElementById('clearChatBtn');
        this.errorMessage = document.getElementById('errorMessage');
        this.quickActions = document.getElementById('quickActions');
    }

    attachEventListeners() {
        // Form submission
        this.chatForm.addEventListener('submit', (e) => {
            e.preventDefault();
            this.sendMessage();
        });

        // Input validation
        this.messageInput.addEventListener('input', () => {
            this.updateSendButtonState();
            this.autoResizeTextarea();
        });

        // User ID change
        this.userIdInput.addEventListener('change', () => {
            const newUserId = parseInt(this.userIdInput.value);
            if (newUserId > 0) {
                this.userId = newUserId;
                this.loadChatHistory();
            }
        });

        // Load history button
        this.loadHistoryBtn.addEventListener('click', () => {
            this.loadChatHistory();
        });

        // Clear chat button
        this.clearChatBtn.addEventListener('click', () => {
            if (confirm('Are you sure you want to clear the chat?')) {
                this.clearChat();
            }
        });

        // Quick action buttons
        this.quickActions.addEventListener('click', (e) => {
            if (e.target.classList.contains('quick-action-btn')) {
                const agentType = e.target.dataset.agent;
                this.triggerAgent(agentType);
            }
        });

        // Keyboard shortcuts
        this.messageInput.addEventListener('keydown', (e) => {
            if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                if (!this.sendBtn.disabled) {
                    this.sendMessage();
                }
            }
        });
    }

    autoResizeTextarea() {
        this.messageInput.style.height = 'auto';
        this.messageInput.style.height = Math.min(this.messageInput.scrollHeight, 120) + 'px';
    }

    updateSendButtonState() {
        const hasText = this.messageInput.value.trim().length > 0;
        this.sendBtn.disabled = !hasText || this.isLoading;
    }

    async sendMessage() {
        const message = this.messageInput.value.trim();
        if (!message || this.isLoading) return;

        // Add user message to chat
        this.addMessage('user', message, 'USER_MESSAGE');
        this.messageInput.value = '';
        this.updateSendButtonState();
        this.autoResizeTextarea();

        // Show loading state
        const loadingId = this.addLoadingMessage();

        try {
            const response = await fetch(`${this.apiBase}/chat/message`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    user_id: this.userId,
                    message: message
                })
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();
            
            // Remove loading message
            this.removeMessage(loadingId);
            
            // Add agent response
            this.addMessage('agent', data.message, data.agent_type || 'AGENT_RESPONSE');
            this.hideError();
        } catch (error) {
            this.removeMessage(loadingId);
            this.showError(`Failed to send message: ${error.message}`);
            console.error('Error sending message:', error);
        }
    }

    async triggerAgent(agentType) {
        if (this.isLoading) return;

        // Disable quick action buttons
        const buttons = this.quickActions.querySelectorAll('.quick-action-btn');
        buttons.forEach(btn => btn.disabled = true);

        // Show loading state
        const loadingId = this.addLoadingMessage();

        try {
            const response = await fetch(`${this.apiBase}/agents/run`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    user_id: this.userId,
                    agent_type: agentType
                })
            });

            if (!response.ok) {
                throw new Error(`HTTP error! status: ${response.status}`);
            }

            const data = await response.json();
            
            // Remove loading message
            this.removeMessage(loadingId);
            
            // Add agent message
            this.addMessage('agent', data.message, data.agent_type);
            this.hideError();
        } catch (error) {
            this.removeMessage(loadingId);
            this.showError(`Failed to trigger agent: ${error.message}`);
            console.error('Error triggering agent:', error);
        } finally {
            // Re-enable buttons
            buttons.forEach(btn => btn.disabled = false);
        }
    }

    async loadChatHistory() {
        try {
            const response = await fetch(`${this.apiBase}/chat/history/${this.userId}?limit=50`);
            
            if (!response.ok) {
                // If error, just show empty history instead of throwing
                console.warn(`Warning: Could not load chat history (${response.status}). Showing empty history.`);
                this.clearChat();
                return;
            }

            const data = await response.json();
            
            // Clear existing messages (except welcome)
            const welcomeMsg = this.chatMessages.querySelector('.welcome-message');
            this.chatMessages.innerHTML = '';
            if (welcomeMsg) {
                this.chatMessages.appendChild(welcomeMsg);
            }

            // Add messages from history
            if (data.messages && data.messages.length > 0) {
                data.messages.reverse().forEach(msg => {
                    const role = msg.agent_type === 'USER_MESSAGE' ? 'user' : 'agent';
                    this.addMessage(role, msg.message, msg.agent_type, msg.created_at);
                });
            }
            
            this.hideError();
        } catch (error) {
            this.showError(`Failed to load chat history: ${error.message}`);
            console.error('Error loading chat history:', error);
        }
    }

    addMessage(role, content, agentType, timestamp = null) {
        // Remove welcome message if it exists
        const welcomeMsg = this.chatMessages.querySelector('.welcome-message');
        if (welcomeMsg) {
            welcomeMsg.remove();
        }

        const messageDiv = document.createElement('div');
        messageDiv.className = `message ${role}`;
        messageDiv.setAttribute('data-role', role);
        
        const messageId = `msg-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
        messageDiv.id = messageId;

        const messageContent = document.createElement('div');
        messageContent.className = 'message-content';
        messageContent.textContent = content;

        const messageMeta = document.createElement('div');
        messageMeta.className = 'message-meta';
        
        const messageType = document.createElement('div');
        messageType.className = 'message-type';
        messageType.textContent = agentType || (role === 'user' ? 'You' : 'Agent');
        
        const messageTime = document.createElement('div');
        messageTime.className = 'message-time';
        messageTime.textContent = timestamp 
            ? new Date(timestamp).toLocaleTimeString() 
            : new Date().toLocaleTimeString();

        messageMeta.appendChild(messageType);
        messageMeta.appendChild(messageTime);
        messageDiv.appendChild(messageContent);
        messageDiv.appendChild(messageMeta);

        this.chatMessages.appendChild(messageDiv);
        this.scrollToBottom();

        return messageId;
    }

    addLoadingMessage() {
        const messageDiv = document.createElement('div');
        messageDiv.className = 'message agent loading';
        const messageId = `loading-${Date.now()}`;
        messageDiv.id = messageId;

        const messageContent = document.createElement('div');
        messageContent.className = 'message-content';
        messageContent.innerHTML = `
            <span>Agent is thinking</span>
            <div class="loading-dots">
                <span></span>
                <span></span>
                <span></span>
            </div>
        `;

        const messageMeta = document.createElement('div');
        messageMeta.className = 'message-meta';
        messageMeta.innerHTML = `
            <div class="message-type">Agent</div>
            <div class="message-time">${new Date().toLocaleTimeString()}</div>
        `;

        messageDiv.appendChild(messageContent);
        messageDiv.appendChild(messageMeta);
        this.chatMessages.appendChild(messageDiv);
        this.scrollToBottom();

        return messageId;
    }

    removeMessage(messageId) {
        const message = document.getElementById(messageId);
        if (message) {
            message.remove();
        }
    }

    clearChat() {
        const welcomeMsg = document.createElement('div');
        welcomeMsg.className = 'welcome-message';
        welcomeMsg.innerHTML = `
            <p>👋 Welcome to CRM Agent Chat!</p>
            <p>Start a conversation by typing a message below.</p>
        `;
        
        this.chatMessages.innerHTML = '';
        this.chatMessages.appendChild(welcomeMsg);
        this.hideError();
    }

    scrollToBottom() {
        this.chatMessages.scrollTop = this.chatMessages.scrollHeight;
    }

    showError(message) {
        this.errorMessage.textContent = message;
        this.errorMessage.classList.add('show');
        setTimeout(() => {
            this.hideError();
        }, 5000);
    }

    hideError() {
        this.errorMessage.classList.remove('show');
    }
}

// Initialize chat interface when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new ChatInterface();
});

