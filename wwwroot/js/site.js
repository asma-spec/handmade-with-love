// Fermeture auto des toasts après 4s
document.addEventListener('DOMContentLoaded', function () {
    const toasts = document.querySelectorAll('.alert-toast');
    toasts.forEach(toast => {
        setTimeout(() => {
            toast.style.animation = 'slideIn 0.4s ease reverse';
            setTimeout(() => toast.remove(), 400);
        }, 4000);
    });
});

// Mise à jour compteur panier
function updateCartCount(count) {
    const badge = document.querySelector('.cart-badge');
    if (badge) badge.textContent = count;
}
// ══════════════════════════════════════════
// WIDGET CHAT
// ══════════════════════════════════════════
function toggleChat() {
    const win = document.getElementById('chatWindow');
    const icon = document.getElementById('chatFabIcon');
    const notif = document.getElementById('chatNotif');

    win.classList.toggle('open');

    if (win.classList.contains('open')) {
        icon.className = 'fas fa-times';
        notif.style.display = 'none';
        scrollChat();
    } else {
        icon.className = 'fas fa-comments';
    }
}

function sendMessage() {
    const input = document.getElementById('chatInput');
    const msg = input.value.trim();
    if (!msg) return;

    appendMessage(msg, 'client');
    input.value = '';
    showTyping();

    // Déterminer l'endpoint selon connexion
    const isLoggedIn = document.querySelector('[data-userid]') !== null;
    const endpoint = isLoggedIn
        ? '/Chat/Send'
        : '/Chat/SendAnonymous';

    fetch(endpoint, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ message: msg })
    })
        .then(r => r.json())
        .then(data => {
            removeTyping();
            if (data.success) {
                setTimeout(() => appendMessage(data.reply, 'bot', data.sentAt), 300);
            }
        })
        .catch(() => {
            removeTyping();
            appendMessage("Désolé, une erreur s'est produite. Réessayez.", 'bot');
        });
}

function sendSuggest(text) {
    document.getElementById('chatInput').value = text;
    sendMessage();
    document.querySelector('.chat-suggestions')?.remove();
}

function appendMessage(text, sender, time) {
    const container = document.getElementById('chatMessages');
    const now = time || new Date().toLocaleTimeString('fr-FR',
        { hour: '2-digit', minute: '2-digit' });

    const div = document.createElement('div');
    div.className = 'chat-msg ' + sender;
    div.innerHTML =
        '<div class="chat-bubble">' + text + '</div>' +
        '<div class="chat-time">' + now + '</div>';

    container.appendChild(div);
    scrollChat();
}

function showTyping() {
    const container = document.getElementById('chatMessages');
    const div = document.createElement('div');
    div.className = 'chat-msg bot chat-typing';
    div.id = 'typingIndicator';
    div.innerHTML =
        '<div class="chat-bubble">' +
        '<div class="typing-dot"></div>' +
        '<div class="typing-dot"></div>' +
        '<div class="typing-dot"></div>' +
        '</div>';
    container.appendChild(div);
    scrollChat();
}

function removeTyping() {
    document.getElementById('typingIndicator')?.remove();
}

function scrollChat() {
    const container = document.getElementById('chatMessages');
    if (container) container.scrollTop = container.scrollHeight;
}
// Mise à jour compteur panier au chargement
document.addEventListener('DOMContentLoaded', function () {
    fetch('/Cart/GetCount')
        .then(r => r.json())
        .then(data => {
            const badge = document.querySelector('.cart-badge');
            if (badge) badge.textContent = data.count;
        })
        .catch(() => { });
});