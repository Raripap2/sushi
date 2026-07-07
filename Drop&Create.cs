using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyDataBase
{
    public class Drop_Create
    {
        public string DropAndCreateSQL()
        {
            string _sqlDropACreate = @"
DROP TABLE IF EXISTS users CASCADE;
DROP TABLE IF EXISTS sushi CASCADE;
DROP TABLE IF EXISTS sushi_photo CASCADE;
DROP TABLE IF EXISTS sushi_type CASCADE;
DROP TABLE IF EXISTS users_status CASCADE;
DROP TABLE IF EXISTS users_orders CASCADE;
DROP TABLE IF EXISTS orders_detail CASCADE;
DROP TABLE IF EXISTS orders_items CASCADE;
DROP TABLE IF EXISTS users_address CASCADE;
DROP TABLE IF EXISTS users_photo CASCADE;
DROP TABLE IF EXISTS users_cart CASCADE;
DROP TABLE IF EXISTS cart_detail CASCADE;


CREATE TABLE users (
    id SERIAL PRIMARY KEY,
    login TEXT,
    email TEXT,
    username TEXT,
    password TEXT,
    created_at TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE users_photo (
    id SERIAL PRIMARY KEY,
    user_id INT REFERENCES users(id),
    photo TEXT
);

CREATE TABLE users_status (
    id SERIAL PRIMARY KEY,
    user_id INT REFERENCES users(id),
    status TEXT CHECK (status IN ('admin', 'user', 'worker', 'godmod', 'courier'))
);

CREATE TABLE users_address (
    id SERIAL PRIMARY KEY,
    user_id INTEGER REFERENCES users(id),
    city TEXT,
    street TEXT,
    house TEXT,
    entrance INT,
    floor INT,
    flat INT,
    intercom BOOLEAN
);
    
CREATE TABLE users_cart (
    id SERIAL PRIMARY KEY,
    user_id INTEGER REFERENCES users(id)
);

CREATE TABLE sushi_type (
    id SERIAL PRIMARY KEY,
    type TEXT
);

CREATE TABLE sushi (
    id SERIAL PRIMARY KEY,
    name TEXT,
    description TEXT,
    cost BIGINT,
    type_id INT REFERENCES sushi_type(id),
    count INT,
    weight INT,
    is_available BOOLEAN DEFAULT true
);

CREATE TABLE sushi_photo (
    id SERIAL PRIMARY KEY,
    sushi_id INTEGER REFERENCES sushi(id),
    photo TEXT
);

CREATE TABLE cart_detail (
    id SERIAL PRIMARY KEY,
    sushi_id INTEGER REFERENCES sushi(id),
    cart_id INTEGER REFERENCES users_cart(id),
    quantity INTEGER
);

CREATE TABLE users_orders (
    id SERIAL PRIMARY KEY,
    user_id INTEGER REFERENCES users(id),
    type TEXT CHECK (type IN ('pickup', 'delivery')),
    order_date TIMESTAMP WITH TIME ZONE
);

CREATE TABLE orders_detail (
    id SERIAL PRIMARY KEY,
    order_id INTEGER REFERENCES users_orders(id),
    status TEXT CHECK (status IN ('pending', 'preparing', 'on_the_way', 'delivered', 'cancelled', 'completed')),
    cancelled_time TIMESTAMP WITH TIME ZONE,
    total_amount BIGINT,
    comment TEXT
);
            
CREATE TABLE orders_items (
    id SERIAL PRIMARY KEY,
    order_id INTEGER REFERENCES users_orders(id) ON DELETE CASCADE,
    sushi_id INTEGER REFERENCES sushi(id),
    quantity INTEGER NOT NULL DEFAULT 1,
    price BIGINT
);


INSERT INTO users (login, password, username) VALUES
('admin', 'admin', 'admin'),
('user', 'user', 'username'),
('courier', 'courier', 'Васян');
INSERT INTO users_status (user_id, status) VALUES
(1, 'admin'),
(2, 'user'),
(3, 'courier');
INSERT INTO users_address ( user_id, city, street, house, entrance, floor, flat, intercom) VALUES
(1, 'Москва', 'Павлова', 2, 3, 5, 34, true);

INSERT INTO users_cart (user_id) VALUES (1), (2);
INSERT INTO sushi_type (type) VALUES
('сеты'),
('роллы'),
('темпура'),
('сашими'),
('wok'),
('напитки'),
('другое');

INSERT INTO sushi (name, description, cost, type_id, count, weight) VALUES
-- Сеты
('Сет ""Самурай""', 'Большой сет с лососем, угрем, креветкой и овощами', 1200, 1, 24, 600),
('Сет ""Токио""', 'Классический сет с разными видами роллов и нигири', 950, 1, 20, 500),
('Сет ""Филадельфия""', 'Набор из разных роллов с лососем и сливочным сыром', 850, 1, 16, 450),
('Сет ""Калифорния""', 'Разнообразие роллов с крабом и икрой тобико', 780, 1, 18, 480),
('Сет ""Император""', 'Премиальный сет с угрем, лососем и тунцом', 1500, 1, 30, 700),

-- Роллы
('Филадельфия', 'Классические роллы с лососем, сливочным сыром и огурцом', 450, 2, 8, 200),
('Калифорния', 'Роллы с крабом, авокадо и огурцом, обсыпанные икрой тобико', 380, 2, 8, 200),
('Унаги', 'Роллы с угрем, сливочным сыром и соусом унаги', 520, 2, 8, 200),
('Бонито', 'Роллы с лососем и сливочным сыром, обсыпанные стружкой тунца', 420, 2, 8, 200),
('Чизкани', 'Роллы с лососем, сливочным сыром и сыром креметте', 390, 2, 8, 200),
('Окинава', 'Роллы с креветкой, авокадо и огурцом', 370, 2, 8, 200),
('Ясай', 'Вегетарианские роллы с авокадо, огурцом и перцем', 320, 2, 8, 200),
('Теплый ролл с креветкой', 'Обжаренные роллы с креветкой в соусе унаги', 480, 2, 8, 220),

-- Темпура
('Темпура', 'Роллы в хрустящем кляре с креветкой и овощами', 420, 3, 8, 200),
('Темпура с лососем', 'Хрустящие роллы с лососем и сливочным сыром', 450, 3, 8, 210),
('Темпура с угрем', 'Обжаренные роллы с угрем и соусом унаги', 520, 3, 8, 220),
('Темпура с курицей', 'Хрустящие роллы с курицей и овощами', 380, 3, 8, 200),
('Темпура вегетарианская', 'Обжаренные роллы с овощами', 350, 3, 8, 190),

-- Сашими
('Сашими из лосося', 'Тонко нарезанный свежий лосось подается с васаби и имбирем', 350, 4, 8, 120),
('Сашими из тунца', 'Нежное филе тунца, нарезанное тонкими ломтиками', 380, 4, 8, 120),
('Сашими из угря', 'Подается с соусом унаги и кунжутом', 420, 4, 8, 130),
('Сашими из креветки', 'Свежие тигровые креветки', 320, 4, 8, 110),
('Ассорти сашими', 'Набор из лосося, тунца и креветки', 650, 4, 16, 250),

-- WOK
('WOK с курицей', 'Жареная лапша с курицей и овощами', 320, 5, 1, 350),
('WOK с говядиной', 'Лапша с говядиной и соевым соусом', 380, 5, 1, 350),
('WOK с морепродуктами', 'Лапша с креветками, кальмаром и мидиями', 450, 5, 1, 350),
('WOK овощной', 'Вегетарианский вариант с свежими овощами', 280, 5, 1, 320),
('WOK с тофу', 'Лапша с тофу и овощами в соусе терияки', 300, 5, 1, 330),

-- Напитки
('Зеленый чай', 'Традиционный японский зеленый чай', 150, 6, 1, 300),
('Чай матча', 'Японский чай матча с нежным вкусом', 200, 6, 1, 300),
('Саке', 'Традиционный японский рисовый напиток', 350, 6, 1, 180),
('Асахи', 'Японское светлое пиво', 250, 6, 1, 500),
('Кока-кола', 'Освежающий газированный напиток', 120, 6, 1, 330),
('Сок апельсиновый', 'Свежевыжатый апельсиновый сок', 180, 6, 1, 330),
('Морс клюквенный', 'Освежающий клюквенный морс', 160, 6, 1, 330);

";
            return _sqlDropACreate;
        }


        public string photoSql()
        {
            string _photoData = LoadPhotoDataFromFile("sushi_photo_data.txt");
            return _photoData;
        }

        private string LoadPhotoDataFromFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    return File.ReadAllText(filePath, Encoding.UTF8);
                }
                else
                {
                    // Возвращаем заглушку или пустую строку, если файл не найден
                    return "-- Файл с данными фотографий не найден\n";
                }
            }
            catch (Exception ex)
            {
                return $"-- Ошибка загрузки данных фотографий: {ex.Message}\n";
            }
        }
    }
}
