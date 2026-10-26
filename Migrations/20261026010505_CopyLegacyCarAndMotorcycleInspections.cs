using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiTaller.Migrations
{
    /// <inheritdoc />
    public partial class CopyLegacyCarAndMotorcycleInspections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Folio",
                table: "Inspections",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LegacyId",
                table: "Inspections",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegacySource",
                table: "Inspections",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Inspections",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inspections_LegacySource_LegacyId",
                table: "Inspections",
                columns: new[] { "LegacySource", "LegacyId" },
                unique: true,
                filter: "[LegacySource] IS NOT NULL");

            // Copy (never move) legacy Auto/Moto inspections into the generic
            // engine. Every insert is guarded, so re-running adds nothing.
            migrationBuilder.Sql(@"
-- WorkshopVehicleInspection -> Inspections (TemplateId -10)
INSERT INTO Inspections (TemplateId, WorkshopId, CustomerId, VehicleId, InspectionDate, Status, IsActive,
    GeneralObservations, Title, Folio, LegacySource, LegacyId)
SELECT -10, w.WorkshopId, w.CustomerId, w.VehicleId, w.InspectionDate, w.Status, w.IsActive,
    w.Observations, h.Title, h.Folio, 'WorkshopVehicleInspection', w.Id
FROM WorkshopVehicleInspections w
OUTER APPLY (SELECT TOP 1 x.Title, x.Folio FROM VehicleInspectionHistory x
             WHERE x.VehicleInspectionId = w.Id ORDER BY x.CreatedAt) h
WHERE NOT EXISTS (SELECT 1 FROM Inspections i WHERE i.LegacySource = 'WorkshopVehicleInspection' AND i.LegacyId = w.Id);

INSERT INTO InspectionResultItems (InspectionId, TemplateItemId, Value)
SELECT i.Id, v.ItemId, v.Value
FROM Inspections i
JOIN WorkshopVehicleInspections w ON i.LegacySource = 'WorkshopVehicleInspection' AND i.LegacyId = w.Id
CROSS APPLY (VALUES
        (-174, w.FrontRightBrake),
        (-175, w.FrontRightTireTread),
        (-176, w.FrontRightTireAlignment),
        (-177, w.FrontLeftBrake),
        (-178, w.FrontLeftTireTread),
        (-179, w.FrontLeftTireAlignment),
        (-180, w.RearRightBrake),
        (-181, w.RearRightTireTread),
        (-182, w.RearRightTireAlignment),
        (-183, w.RearLeftBrake),
        (-184, w.RearLeftTireTread),
        (-185, w.RearLeftTireAlignment),
        (-186, w.Brakes),
        (-187, w.TireTread),
        (-188, w.TireAlignment),
        (-189, w.Headlights),
        (-190, w.Taillights),
        (-191, w.TurnSignals),
        (-192, w.BrakeLights),
        (-193, w.HazardLights),
        (-194, w.WindshieldWasherFluid),
        (-195, w.WindshieldWiperOperation),
        (-196, w.WindshieldWiperBlades),
        (-197, w.WindshieldCondition),
        (-198, w.Mirrors),
        (-199, w.EmergencyBrake),
        (-200, w.Horn),
        (-201, w.FuelTankCap),
        (-202, w.AirConditioningFilter),
        (-203, w.ReversingLights),
        (-204, w.LicensePlateLight),
        (-205, w.SeatBelts),
        (-206, w.EngineOilLevel),
        (-207, w.CoolantLevel),
        (-208, w.BrakeFluidLevel),
        (-209, w.AirFilter),
        (-210, w.RadiatorHoses),
        (-211, w.HeatingHoses),
        (-212, w.AirConditioningCondenser),
        (-213, w.TransmissionFluidLevel),
        (-214, w.PowerSteeringFluidLevel),
        (-215, w.AccessoryBelt),
        (-216, w.ExhaustSystem),
        (-217, w.BatteryTerminals),
        (-218, w.BatteryCables),
        (-219, w.BatteryMounting),
        (-220, w.GeneralBatteryCondition),
        (-221, w.FrontShockAbsorbers),
        (-222, w.RearShockAbsorbers),
        (-223, w.BallJoints),
        (-224, w.SteeringRackAndTierods),
        (-225, w.SuspensionBushings)
    ) v(ItemId, Value)
WHERE NOT EXISTS (SELECT 1 FROM InspectionResultItems r WHERE r.InspectionId = i.Id);

INSERT INTO InspectionSectionComments (InspectionId, SectionId, Comment)
SELECT i.Id, c.SectionId, c.Comment
FROM Inspections i
JOIN WorkshopVehicleInspections w ON i.LegacySource = 'WorkshopVehicleInspection' AND i.LegacyId = w.Id
CROSS APPLY (VALUES
        (-27, w.TiresComments),
        (-28, w.InteriorAndExteriorComments),
        (-29, w.EngineComments),
        (-30, w.BatteryComments),
        (-31, w.SuspensionComments),
        (-32, w.ChasisComments)
    ) c(SectionId, Comment)
WHERE ISNULL(c.Comment, '') <> ''
  AND NOT EXISTS (SELECT 1 FROM InspectionSectionComments s WHERE s.InspectionId = i.Id);

INSERT INTO InspectionFiles (InspectionId, FileName, FileType, FileData)
SELECT i.Id, f.FileName, f.FileType, f.FileData
FROM Inspections i
JOIN WorkshopVehicleFiles f ON i.LegacySource = 'WorkshopVehicleInspection' AND f.WorkshopVehicleInspectionId = i.LegacyId
WHERE NOT EXISTS (SELECT 1 FROM InspectionFiles x WHERE x.InspectionId = i.Id);

INSERT INTO InspectionDetailHistories (InspectionId, Title, IsCompleted, CreatedAt, CompletedAt)
SELECT i.Id, d.Title, d.IsCompleted, d.CreatedAt, d.CompletedAt
FROM Inspections i
JOIN VehicleInspectionDetailHistory d ON i.LegacySource = 'WorkshopVehicleInspection' AND d.VehicleInspectionId = i.LegacyId
WHERE NOT EXISTS (SELECT 1 FROM InspectionDetailHistories x WHERE x.InspectionId = i.Id);

-- WorkshopMotocycleInspection -> Inspections (TemplateId -11)
INSERT INTO Inspections (TemplateId, WorkshopId, CustomerId, VehicleId, InspectionDate, Status, IsActive,
    GeneralObservations, Title, Folio, LegacySource, LegacyId)
SELECT -11, w.WorkshopId, w.CustomerId, w.VehicleId, w.InspectionDate, w.Status, w.IsActive,
    w.Observations, h.Title, h.Folio, 'WorkshopMotocycleInspection', w.Id
FROM WorkshopMotocycleInspections w
OUTER APPLY (SELECT TOP 1 x.Title, x.Folio FROM MotocycleInspectionHistory x
             WHERE x.MotocycleInspectionId = w.Id ORDER BY x.CreatedAt) h
WHERE NOT EXISTS (SELECT 1 FROM Inspections i WHERE i.LegacySource = 'WorkshopMotocycleInspection' AND i.LegacyId = w.Id);

INSERT INTO InspectionResultItems (InspectionId, TemplateItemId, Value)
SELECT i.Id, v.ItemId, v.Value
FROM Inspections i
JOIN WorkshopMotocycleInspections w ON i.LegacySource = 'WorkshopMotocycleInspection' AND i.LegacyId = w.Id
CROSS APPLY (VALUES
        (-226, w.FrontRadios),
        (-227, w.FrontTireThreadPattern),
        (-228, w.FrontBearings),
        (-229, w.FrontStamps),
        (-230, w.FrontBrakeLining),
        (-231, w.FrontWearPattern),
        (-232, w.RearRadios),
        (-233, w.RearTireThreadPattern),
        (-234, w.RearBearings),
        (-235, w.RearStamps),
        (-236, w.RearBrakeLining),
        (-237, w.RearWearPattern),
        (-238, w.Headlight),
        (-239, w.Taillight),
        (-240, w.TurnSignals),
        (-241, w.HazardLights),
        (-242, w.Stoplight),
        (-243, w.LicensePlateLight),
        (-244, w.LeftMirror),
        (-245, w.RightMirror),
        (-246, w.Switches),
        (-247, w.Cabling),
        (-248, w.HandleBars),
        (-249, w.LeversAndPedal),
        (-250, w.Hoses),
        (-251, w.ThrottleLever),
        (-252, w.ClutchLever),
        (-253, w.FuelTankCap),
        (-254, w.DashboardInstruments),
        (-255, w.Horn),
        (-256, w.FrameCondition),
        (-257, w.SteeringBearings),
        (-258, w.SwingarmBushings),
        (-259, w.FrontForks),
        (-260, w.RearShockAbsorbers),
        (-261, w.ChainOrStrap),
        (-262, w.Fasteners),
        (-263, w.CentralSupport),
        (-264, w.LateralSupport),
        (-265, w.EngineOil),
        (-266, w.GearOil),
        (-267, w.AxleTransmissionOil),
        (-268, w.HydraulicFluid),
        (-269, w.Refrigerant),
        (-270, w.Fuel),
        (-271, w.Leaks),
        (-272, w.BatteryTerminals),
        (-273, w.Cables),
        (-274, w.Mounting),
        (-275, w.GeneralBatteryConditions)
    ) v(ItemId, Value)
WHERE NOT EXISTS (SELECT 1 FROM InspectionResultItems r WHERE r.InspectionId = i.Id);

INSERT INTO InspectionSectionComments (InspectionId, SectionId, Comment)
SELECT i.Id, c.SectionId, c.Comment
FROM Inspections i
JOIN WorkshopMotocycleInspections w ON i.LegacySource = 'WorkshopMotocycleInspection' AND i.LegacyId = w.Id
CROSS APPLY (VALUES
        (-33, w.TiresComments),
        (-34, w.LightsAndControlsComments),
        (-35, w.FrameAndSuspensionComments),
        (-36, w.OilAndLevelsComments),
        (-37, w.BatteryComments),
        (-38, w.ChasisComments)
    ) c(SectionId, Comment)
WHERE ISNULL(c.Comment, '') <> ''
  AND NOT EXISTS (SELECT 1 FROM InspectionSectionComments s WHERE s.InspectionId = i.Id);

INSERT INTO InspectionFiles (InspectionId, FileName, FileType, FileData)
SELECT i.Id, f.FileName, f.FileType, f.FileData
FROM Inspections i
JOIN MotocycleInspectionFiles f ON i.LegacySource = 'WorkshopMotocycleInspection' AND f.WorkshopMotocycleInspectionId = i.LegacyId
WHERE NOT EXISTS (SELECT 1 FROM InspectionFiles x WHERE x.InspectionId = i.Id);

INSERT INTO InspectionDetailHistories (InspectionId, Title, IsCompleted, CreatedAt, CompletedAt)
SELECT i.Id, d.Title, d.IsCompleted, d.CreatedAt, d.CompletedAt
FROM Inspections i
JOIN MotocycleInspectionDetailHistory d ON i.LegacySource = 'WorkshopMotocycleInspection' AND d.MotocycleInspectionId = i.LegacyId
WHERE NOT EXISTS (SELECT 1 FROM InspectionDetailHistories x WHERE x.InspectionId = i.Id);
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DECLARE @copied TABLE (Id int PRIMARY KEY);
INSERT INTO @copied SELECT Id FROM Inspections WHERE LegacySource IS NOT NULL;
UPDATE Quotations SET InspectionId = NULL WHERE InspectionId IN (SELECT Id FROM @copied);
DELETE FROM InspectionHistories WHERE InspectionId IN (SELECT Id FROM @copied);
DELETE FROM InspectionDetailHistories WHERE InspectionId IN (SELECT Id FROM @copied);
DELETE FROM InspectionFiles WHERE InspectionId IN (SELECT Id FROM @copied);
DELETE FROM InspectionSectionComments WHERE InspectionId IN (SELECT Id FROM @copied);
DELETE FROM InspectionResultItems WHERE InspectionId IN (SELECT Id FROM @copied);
DELETE FROM Inspections WHERE Id IN (SELECT Id FROM @copied);
");

            migrationBuilder.DropIndex(
                name: "IX_Inspections_LegacySource_LegacyId",
                table: "Inspections");

            migrationBuilder.DropColumn(
                name: "Folio",
                table: "Inspections");

            migrationBuilder.DropColumn(
                name: "LegacyId",
                table: "Inspections");

            migrationBuilder.DropColumn(
                name: "LegacySource",
                table: "Inspections");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Inspections");
        }
    }
}
