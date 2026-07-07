async function initPage() {
    updateCartCount();
    updateSession();
}

async function updateCartCount() {
    try {
        const response = await fetch('/api/Sushi/cart/get');
        if (response.ok) {
            const cartData = await response.json();
            const totalItems = cartData.items?.reduce((sum, item) => sum + item.quantity, 0) || 0;
            document.getElementById('cartCount').textContent = totalItems;
        }
    } catch (error) {
        console.error('Ошибка обновления счетчика корзины:', error);
    }
}

function showNotification(message) {
    const notification = document.createElement('div');
    notification.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            background: var(--accent);
            color: white;
            padding: 15px 20px;
            border-radius: 8px;
            z-index: 10000;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        `;
    notification.textContent = message;
    document.body.appendChild(notification);

    setTimeout(() => {
        notification.remove();
    }, 3000);
}

function openCart() {
    window.location.href = '/cart';
}

function openProfile() {
    window.location.href = '/profile';
}

async function loadUserInfo() {
    try {
        const response = await fetch('/api/Users/profile');
        if (response.ok) {
            const userData = await response.json();
            window.userData = userData;

            const username = userData.username || userData.login || 'Гость';
            document.getElementById('username').textContent = `Привет, ${username}`;

            if (userData.photo) {
                document.getElementById('userAvatar').src = userData.photo;
            }
        } else {
            if (response.status === 401) {
                window.location.href = '/login';
            }
        }
    } catch (error) {
        console.log('Ошибка загрузки информации о пользователе:', error);
    }
}

async function logout() {
    try {
        const response = await fetch('/api/Users/logout', {
            method: 'POST'
        });

        const result = await response.json();

        if (result.success) {
            localStorage.removeItem('sushiCart');
            window.location.href = '/login';
        }

    } catch (error) {
        console.error('Ошибка при выходе:', error);
    }
}

// Функция для обновления сессии
async function updateSession() {
    try {
        const response = await fetch('/api/session/update', {
            method: 'GET',
            credentials: 'include' // важно для отправки куки с сессией
        });

        const data = await response.json();

        if (!data.success) {
            console.warn('Сессия истекла:', data.message);
            // Перенаправляем на страницу входа
            window.location.href = '/login';
        } else {
            console.log('Сессия обновлена');
        }
    } catch (error) {
        console.error('Ошибка при обновлении сессии:', error);
    }
}

// Запускаем обновление каждые 20 минут (1200000 ms)
const SESSION_UPDATE_INTERVAL = 20 * 60 * 1000; // 20 минут
let sessionInterval = setInterval(updateSession, SESSION_UPDATE_INTERVAL);