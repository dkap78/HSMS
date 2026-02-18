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


-- STORED PROCEDURES
CREATE OR REPLACE FUNCTION sp_users_insert(
    p_email TEXT,
    p_password_hash TEXT,
    p_first_name TEXT,
    p_last_name TEXT,
    p_phone TEXT
)
RETURNS UUID
LANGUAGE plpgsql
AS $$
DECLARE
    v_id UUID := uuid_generate_v4();
BEGIN
    INSERT INTO users (
        id, email, password_hash, first_name, last_name, phone,
        is_active, created_at, updated_at
    )
    VALUES (
        v_id, p_email, p_password_hash, p_first_name, p_last_name, p_phone,
        TRUE, NOW(), NOW()
    );

    RETURN v_id;
END;
$$;

CREATE OR REPLACE FUNCTION sp_users_get_by_id(p_id UUID)
RETURNS SETOF users
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT *
    FROM users
    WHERE id = p_id
      AND is_active = TRUE;
END;
$$;

CREATE OR REPLACE FUNCTION sp_users_get_by_email(p_email TEXT)
RETURNS SETOF users
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT *
    FROM users
    WHERE email = p_email
      AND is_active = TRUE;
END;
$$;

CREATE OR REPLACE PROCEDURE sp_users_update(
    p_id UUID,
    p_first_name TEXT,
    p_last_name TEXT,
    p_phone TEXT
)
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE users
    SET first_name = p_first_name,
        last_name  = p_last_name,
        phone      = p_phone,
        updated_at = NOW()
    WHERE id = p_id;
END;
$$;

CREATE OR REPLACE PROCEDURE sp_users_soft_delete(p_id UUID)
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE users
    SET is_active = FALSE,
        updated_at = NOW()
    WHERE id = p_id;
END;
$$;

CREATE OR REPLACE FUNCTION sp_flats_insert(
    p_flat_number TEXT,
    p_wing TEXT,
    p_floor INT
)
RETURNS UUID
LANGUAGE plpgsql
AS $$
DECLARE
    v_id UUID := uuid_generate_v4();
BEGIN
    INSERT INTO flats (
        id, flat_number, wing, floor,
        is_active, created_at, updated_at
    )
    VALUES (
        v_id, p_flat_number, p_wing, p_floor,
        TRUE, NOW(), NOW()
    );

    RETURN v_id;
END;
$$;

CREATE OR REPLACE PROCEDURE sp_flat_occupancy_insert(
    p_flat_id UUID,
    p_owner_user_id UUID,
    p_tenant_user_id UUID,
    p_occupancy_type occupancy_type,
    p_start_date DATE
)
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE flat_occupancy
    SET is_active = FALSE
    WHERE flat_id = p_flat_id;

    INSERT INTO flat_occupancy (
        id, flat_id, owner_user_id, tenant_user_id,
        occupancy_type, start_date, is_active
    )
    VALUES (
        uuid_generate_v4(),
        p_flat_id, p_owner_user_id, p_tenant_user_id,
        p_occupancy_type, p_start_date, TRUE
    );
END;
$$;

CREATE OR REPLACE FUNCTION sp_flat_directory()
RETURNS TABLE (
    flat_id UUID,
    flat_number TEXT,
    wing TEXT,
    floor INT,
    occupancy_type occupancy_type,
    owner_id UUID,
    tenant_id UUID
)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT
        f.id, f.flat_number, f.wing, f.floor,
        fo.occupancy_type,
        fo.owner_user_id,
        fo.tenant_user_id
    FROM flats f
    JOIN flat_occupancy fo ON fo.flat_id = f.id
    WHERE f.is_active = TRUE
      AND fo.is_active = TRUE;
END;
$$;

CREATE OR REPLACE FUNCTION sp_notices_insert(
    p_title TEXT,
    p_content TEXT,
    p_created_by UUID,
    p_expire_date DATE
)
RETURNS UUID
LANGUAGE plpgsql
AS $$
DECLARE
    v_id UUID := uuid_generate_v4();
BEGIN
    INSERT INTO notices (
        id, title, content, created_by_user_id,
        publish_date, expire_date, is_active,
        created_at, updated_at
    )
    VALUES (
        v_id, p_title, p_content, p_created_by,
        CURRENT_DATE, p_expire_date, TRUE,
        NOW(), NOW()
    );

    RETURN v_id;
END;
$$;

CREATE OR REPLACE FUNCTION sp_notices_list()
RETURNS SETOF notices
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT *
    FROM notices
    WHERE is_active = TRUE
      AND (expire_date IS NULL OR expire_date >= CURRENT_DATE)
    ORDER BY publish_date DESC;
END;
$$;

CREATE OR REPLACE FUNCTION sp_payments_insert(
    p_user_id UUID,
    p_flat_id UUID,
    p_amount NUMERIC,
    p_payment_type payment_type,
    p_transaction_ref TEXT,
    p_payment_date DATE,
    p_entered_by UUID
)
RETURNS UUID
LANGUAGE plpgsql
AS $$
DECLARE
    v_id UUID := uuid_generate_v4();
BEGIN
    INSERT INTO payments (
        id, user_id, flat_id, amount,
        payment_type, transaction_ref,
        payment_date, status,
        entered_by_user_id,
        created_at, updated_at
    )
    VALUES (
        v_id, p_user_id, p_flat_id, p_amount,
        p_payment_type, p_transaction_ref,
        p_payment_date, 'Pending',
        p_entered_by,
        NOW(), NOW()
    );

    RETURN v_id;
END;
$$;

CREATE OR REPLACE PROCEDURE sp_payments_approve(
    p_payment_id UUID,
    p_admin_id UUID
)
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE payments
    SET status = 'Approved',
        approved_by_user_id = p_admin_id,
        approval_date = NOW(),
        updated_at = NOW()
    WHERE id = p_payment_id;
END;
$$;

CREATE OR REPLACE PROCEDURE sp_payments_reject(
    p_payment_id UUID,
    p_admin_id UUID,
    p_remarks TEXT
)
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE payments
    SET status = 'Rejected',
        approved_by_user_id = p_admin_id,
        approval_date = NOW(),
        remarks = p_remarks,
        updated_at = NOW()
    WHERE id = p_payment_id;
END;
$$;

CREATE OR REPLACE FUNCTION sp_accounts_insert(
    p_name TEXT,
    p_type account_type
)
RETURNS UUID
LANGUAGE plpgsql
AS $$
DECLARE
    v_id UUID := uuid_generate_v4();
BEGIN
    INSERT INTO accounts (id, name, type, is_active)
    VALUES (v_id, p_name, p_type, TRUE);

    RETURN v_id;
END;
$$;

CREATE OR REPLACE PROCEDURE sp_account_transactions_insert(
    p_account_id UUID,
    p_amount NUMERIC,
    p_transaction_date DATE,
    p_reference TEXT,
    p_description TEXT,
    p_created_by UUID
)
LANGUAGE plpgsql
AS $$
BEGIN
    INSERT INTO account_transactions (
        id, account_id, amount,
        transaction_date, reference,
        description, created_by_user_id,
        created_at, updated_at
    )
    VALUES (
        uuid_generate_v4(),
        p_account_id, p_amount,
        p_transaction_date, p_reference,
        p_description, p_created_by,
        NOW(), NOW()
    );
END;
$$;

CREATE OR REPLACE PROCEDURE sp_audit_insert(
    p_entity_name TEXT,
    p_entity_id UUID,
    p_action audit_action,
    p_changed_by UUID,
    p_old_values JSONB,
    p_new_values JSONB
)
LANGUAGE plpgsql
AS $$
BEGIN
    INSERT INTO audit_logs (
        id, entity_name, entity_id, action,
        changed_by_user_id, changed_at,
        old_values, new_values
    )
    VALUES (
        uuid_generate_v4(),
        p_entity_name, p_entity_id, p_action,
        p_changed_by, NOW(),
        p_old_values, p_new_values
    );
END;
$$;

CREATE OR REPLACE FUNCTION sp_users_search(p_term TEXT)
RETURNS SETOF users AS $$
BEGIN
    RETURN QUERY
    SELECT *
    FROM users
    WHERE is_active = TRUE
      AND (email ILIKE '%' || p_term || '%'
       OR first_name ILIKE '%' || p_term || '%'
       OR last_name ILIKE '%' || p_term || '%');
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_audit_list(
    p_entity TEXT,
    p_from DATE,
    p_to DATE
)
RETURNS SETOF audit_logs
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT *
    FROM audit_logs
    WHERE entity_name = p_entity
      AND changed_at BETWEEN p_from AND p_to
    ORDER BY changed_at DESC;
END;
$$;

CREATE OR REPLACE FUNCTION sp_user_permissions(p_user_id UUID)
RETURNS TABLE(permission TEXT)
AS $$
BEGIN
    RETURN QUERY
    SELECT rp.permission_name
    FROM user_roles ur
    JOIN role_permissions rp ON ur.role_id = rp.role_id
    WHERE ur.user_id = p_user_id;
END;
$$ LANGUAGE plpgsql;

CREATE OR REPLACE FUNCTION sp_roles_insert(p_name TEXT)
RETURNS UUID
LANGUAGE plpgsql
AS $$
DECLARE
    v_id UUID := uuid_generate_v4();
BEGIN
    INSERT INTO roles(id, name)
    VALUES (v_id, p_name);

    RETURN v_id;
END;
$$;

CREATE OR REPLACE FUNCTION sp_roles_list()
RETURNS SETOF roles
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY SELECT * FROM roles ORDER BY name;
END;
$$;

CREATE OR REPLACE FUNCTION sp_user_permissions(p_user_id UUID)
RETURNS TABLE(permission TEXT)
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT rp.permission_name
    FROM user_roles ur
    JOIN role_permissions rp ON ur.role_id = rp.role_id
    WHERE ur.user_id = p_user_id;
END;
$$;


CREATE OR REPLACE FUNCTION sp_payments_get_by_id(p_id UUID)
RETURNS SETOF payments
LANGUAGE plpgsql
AS $$
BEGIN
    RETURN QUERY
    SELECT *
    FROM payments
    WHERE id = p_id;
END;
$$;


