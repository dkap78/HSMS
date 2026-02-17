/*** Tables ***/

-- EXTENSIONS functions to generate various types of Universally Unique Identifiers (UUIDs)
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- ENUM Types
CREATE TYPE occupancy_type AS ENUM ('OwnerOccupied', 'Rented');
CREATE TYPE payment_status AS ENUM ('Pending', 'Approved', 'Rejected');
CREATE TYPE payment_type AS ENUM ('Cash', 'Cheque', 'Online');
CREATE TYPE account_type AS ENUM ('Income', 'Expense');
CREATE TYPE audit_action AS ENUM ('INSERT', 'UPDATE', 'DELETE');

-- Users & Role Management
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash TEXT NOT NULL,
    first_name VARCHAR(100),
    last_name VARCHAR(100),
    phone VARCHAR(20),
    profile_photo_url TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE roles (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(100) NOT NULL UNIQUE,
    created_at TIMESTAMP DEFAULT NOW()
);
CREATE TABLE user_roles (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id),
    role_id UUID NOT NULL REFERENCES roles(id),
    assigned_at TIMESTAMP NOT NULL DEFAULT NOW(),
    assigned_by_user_id UUID REFERENCES users(id),
    UNIQUE (user_id, role_id)
);
CREATE TABLE permissions (
    name VARCHAR(100) PRIMARY KEY
);
CREATE TABLE role_permissions (
    role_id UUID NOT NULL REFERENCES roles(id) ON DELETE CASCADE,
    permission_name VARCHAR(100) NOT NULL REFERENCES permissions(name),
    PRIMARY KEY (role_id, permission_name)
);


-- Seed Roles
INSERT INTO roles (name)
VALUES ('Resident'), ('President'), ('Director'), ('Treasurer');

-- Seed Permissions
INSERT INTO permissions (name) VALUES
('ManageUsers'),
('ManageFlats'),
('ManageNotices'),
('ManageAccounting'),
('ApprovePayments'),
('ViewAccounting');


-- Flats & Occupancy
CREATE TABLE flats (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    flat_number VARCHAR(20) NOT NULL,
    wing VARCHAR(10),
    floor INTEGER,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    UNIQUE (flat_number, wing)
);

CREATE TABLE flat_occupancy (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    flat_id UUID NOT NULL REFERENCES flats(id),
    owner_user_id UUID NOT NULL REFERENCES users(id),
    tenant_user_id UUID REFERENCES users(id),
    occupancy_type occupancy_type NOT NULL,
    start_date DATE NOT NULL,
    end_date DATE,
    is_active BOOLEAN DEFAULT TRUE
);

-- Notices
CREATE TABLE notices (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    title VARCHAR(255) NOT NULL,
    content TEXT NOT NULL,
    created_by_user_id UUID NOT NULL REFERENCES users(id),
    publish_date DATE DEFAULT CURRENT_DATE,
    expire_date DATE,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

CREATE TABLE notice_subscriptions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id),
    notice_id UUID NOT NULL REFERENCES notices(id),
    subscribed_at TIMESTAMP DEFAULT NOW(),
    UNIQUE (user_id, notice_id)
);

-- Events
CREATE TABLE events (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    title VARCHAR(255) NOT NULL,
    description TEXT,
    event_date TIMESTAMP NOT NULL,
    event_end_date TIMESTAMP,
    location VARCHAR(255),
    created_by_user_id UUID NOT NULL REFERENCES users(id),
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Payments
CREATE TABLE payments (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id),
    flat_id UUID NOT NULL REFERENCES flats(id),
    amount NUMERIC(12,2) NOT NULL,
    payment_type payment_type NOT NULL,
    transaction_ref VARCHAR(100),
    payment_date DATE NOT NULL,
    status payment_status DEFAULT 'Pending',
    entered_by_user_id UUID NOT NULL REFERENCES users(id),
    approved_by_user_id UUID REFERENCES users(id),
    approval_date TIMESTAMP,
    remarks TEXT,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Accounting
CREATE TABLE accounts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    name VARCHAR(255) NOT NULL,
    type account_type NOT NULL,
    is_active BOOLEAN DEFAULT TRUE,
    UNIQUE (name, type)
);

CREATE TABLE account_transactions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    account_id UUID NOT NULL REFERENCES accounts(id),
    amount NUMERIC(12,2) NOT NULL,
    transaction_date DATE NOT NULL,
    reference VARCHAR(100),
    description TEXT,
    created_by_user_id UUID NOT NULL REFERENCES users(id),
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Audit & Logs
CREATE TABLE audit_logs (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    entity_name VARCHAR(100) NOT NULL,
    entity_id UUID NOT NULL,
    action audit_action NOT NULL,
    changed_by_user_id UUID NOT NULL REFERENCES users(id),
    changed_at TIMESTAMP DEFAULT NOW(),
    old_values JSONB,
    new_values JSONB
);

-- User Devices (Push Notification)
CREATE TABLE user_devices (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    user_id UUID NOT NULL REFERENCES users(id),
    device_token TEXT NOT NULL,
    platform VARCHAR(20), -- Android / iOS / Web
    is_active BOOLEAN DEFAULT TRUE,
    last_used_at TIMESTAMP
);


-- INDEXES
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_payments_status ON payments(status);
CREATE INDEX idx_audit_entity ON audit_logs(entity_name, entity_id);
CREATE INDEX idx_flat_occupancy_flat ON flat_occupancy(flat_id);
CREATE INDEX idx_account_tx_date ON account_transactions(transaction_date);

