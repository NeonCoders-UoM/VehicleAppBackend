DELETE FROM VehicleServiceHistories WHERE VehicleId = 1;

-- Verified services (GREEN)
INSERT INTO VehicleServiceHistories (VehicleId, ServiceType, Description, Cost, ServiceDate, Mileage, IsVerified, ExternalServiceCenterName)
VALUES 
(1, 'Oil Change', 'Engine oil change', 75.50, DATEADD(day, -30, GETDATE()), 25000, 1, 'Official Service Center'),
(1, 'Brake Service', 'Brake pad replacement', 185.00, DATEADD(day, -55, GETDATE()), 24500, 1, 'Official Service Center'),
(1, 'Annual Inspection', 'Safety inspection', 95.00, DATEADD(day, -95, GETDATE()), 24000, 1, 'Official Service Center');

-- Unverified services (RED)
INSERT INTO VehicleServiceHistories (VehicleId, ServiceType, Description, Cost, ServiceDate, Mileage, IsVerified, ExternalServiceCenterName)
VALUES 
(1, 'Tire Rotation', 'Tire rotation', 45.00, DATEADD(day, -15, GETDATE()), 24800, 0, 'Quick Tire Shop'),
(1, 'Car Wash', 'Car cleaning', 25.00, DATEADD(day, -20, GETDATE()), 24600, 0, 'Sparkle Car Wash'),
(1, 'Battery Check', 'Battery test', 15.00, DATEADD(day, -45, GETDATE()), 24200, 0, 'AutoZone'),
(1, 'Air Filter', 'Filter replacement', 35.00, DATEADD(day, -75, GETDATE()), 24100, 0, 'Local Garage');
