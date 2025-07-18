CREATE KEYSPACE IF NOT EXISTS orders_keyspace
WITH replication = {'class': 'SimpleStrategy', 'replication_factor': 1};

CREATE TABLE IF NOT EXISTS orders_keyspace.orders (
    clOrdId text PRIMARY KEY,
    instId text,
    tdMode text,
    side text,
    ordType text,
    sz text,
    px text,
    status text
); 