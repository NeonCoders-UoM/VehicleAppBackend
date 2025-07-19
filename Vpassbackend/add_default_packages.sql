-- Add default packages (Platinum, Gold, Silver)
INSERT INTO Packages (PackageName, Percentage, Description, IsActive) 
VALUES 
    ('Platinum', 25.00, 'Premium package with 25% discount on services', 1),
    ('Gold', 15.00, 'Standard package with 15% discount on services', 1),
    ('Silver', 10.00, 'Basic package with 10% discount on services', 1); 