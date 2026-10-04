using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;






public static partial class LevelAssetFactory
{
    static Dictionary<string, (string model, Func<GameObject> build)> modelBuilders;

    
    static Dictionary<string, (string model, Func<GameObject> build)> ModelBuilders => modelBuilders ??= new Dictionary<string, (string, Func<GameObject>)>
    {
        [$"{OfficeProps}/Workstation.prefab"] = ("OfficeDesk", WorkstationModel),
        [$"{OfficeProps}/LaptopDesk.prefab"] = ("Laptop", LaptopDeskModel),
        [$"{OfficeProps}/OfficeChair.prefab"] = ("OfficeChair", () => Single("OfficeChair", new Vector3(0.65f, 1.2f, 0.65f))),
        [$"{OfficeProps}/FilingCabinet.prefab"] = ("FilingCabinet", () => Single("FilingCabinet", new Vector3(0.5f, 1.32f, 0.6f))),
        [$"{OfficeProps}/BoxStack.prefab"] = ("PaperBoxes", () => Single("PaperBoxes", new Vector3(0.75f, 0.32f, 0.85f))),
        [$"{TradingDir}/TradingPod.prefab"] = ("BoardroomTable", TradingPodModel),
        [$"{TradingDir}/ExecutiveDesk.prefab"] = ("WoodenTable", ExecutiveDeskModel),
        [$"{TradingDir}/LeatherChair.prefab"] = ("LeatherChair", () => Single("LeatherChair", new Vector3(0.7f, 1.2f, 0.8f))),
        [$"{TradingDir}/PottedPlant.prefab"] = ("PlantPot", () => Single("PlantPot", new Vector3(0.8f, 1.7f, 0.8f))),
        [$"{TradingDir}/Boardroom.prefab"] = ("MeetingTable", BoardroomModel),
        [$"{ArchiveDir}/FilingBank.prefab"] = ("FileCabinets", () => Single("FileCabinets", new Vector3(1.56f, 1.56f, 0.7f))),
        [$"{ArchiveDir}/CardCatalog.prefab"] = ("Lockers", () => Single("Lockers", new Vector3(1.3f, 1.9f, 0.65f))),
        [$"{HospitalDir}/Gurney.prefab"] = ("Gurney", () => Single("Gurney", new Vector3(1.95f, 1.05f, 0.65f), yaw: 90f)),
        [$"{HospitalDir}/Wheelchair.prefab"] = ("Wheelchair", () => Single("Wheelchair", new Vector3(0.65f, 1.0f, 0.95f), yaw: 180f)),
        [$"{HospitalDir}/CrashCart.prefab"] = ("MedicalCart", () => Single("MedicalCart", new Vector3(0.85f, 1.1f, 0.55f))),
        [$"{HospitalDir}/IVStand.prefab"] = ("DripStand", () => Single("DripStand", new Vector3(0.55f, 1.95f, 0.55f))),
        [$"{HospitalDir}/Clipboard.prefab"] = ("Notepad", () => Single("Notepad", new Vector3(0.23f, 0.03f, 0.32f))),
        [$"{HospitalDir}/Syringe.prefab"] = ("Syringe", () => Single("Syringe", new Vector3(0.04f, 0.03f, 0.16f), baseEuler: new Vector3(90f, 0f, 0f))),
        [$"{HospitalDir}/GauzePad.prefab"] = ("GauzePads", () => Single("GauzePads", new Vector3(0.12f, 0.02f, 0.12f))),
        [$"{HospitalDir}/PillBottle.prefab"] = ("MedBox", () => Single("MedBox", new Vector3(0.13f, 0.05f, 0.1f))),
        [$"{FurnaceDir}/Boiler.prefab"] = ("VintageBoiler", () => Single("VintageBoiler", new Vector3(2.3f, FurnaceWallHeight - 0.05f, 1.7f))),
        [$"{FurnaceDir}/FuelDrums.prefab"] = ("RustedMachine", () => Single("RustedMachine", new Vector3(1.1f, 1.7f, 1.1f), keep: "tank007")),
    };

    
    static Func<GameObject> ModelBuilderFor(string path) =>
        ModelBuilders.TryGetValue(path, out var entry) && PropModels.Has(entry.model) ? entry.build : null;

    
    static Transform Model(Transform r, string name, Vector3 bottomCentre, Vector3 maxSize, float yaw = 0f, Vector3 baseEuler = default, string keep = null) =>
        PropModels.Place(r, name, bottomCentre, maxSize, yaw, baseEuler, keep);

    
    static float Top(Transform placed) => placed.localPosition.y + PropModels.LocalBounds(placed).max.y;

    static Bounds ModelBounds(Transform placed)
    {
        Bounds b = PropModels.LocalBounds(placed);
        b.center += placed.localPosition;
        return b;
    }

    static GameObject Single(string name, Vector3 maxSize, float yaw = 0f, Vector3 baseEuler = default, string keep = null)
    {
        Transform r = Root(name);
        Model(r, name, Vector3.zero, maxSize, yaw, baseEuler, keep);
        return r.gameObject;
    }

    
    
    static void MonitorScreen(Transform monitor, float facing, out Vector3 centre, out Vector3 size, out Vector3 euler)
    {
        Bounds b = ModelBounds(monitor);
        size = new Vector3(b.size.x * 0.9f, b.size.y * 0.6f, 0.004f);
        float y = b.min.y + b.size.y * 0.66f;
        float z = facing > 0f ? b.max.z - b.size.z * 0.2f : b.min.z + b.size.z * 0.2f;
        centre = new Vector3(b.center.x, y, z);
        euler = facing > 0f ? new Vector3(-6f, 0f, 0f) : new Vector3(6f, 180f, 0f);
    }

    static GameObject WorkstationModel()
    {
        Transform r = Root("Workstation");
        Transform desk = Model(r, "OfficeDesk", Vector3.zero, new Vector3(1.4f, 0.76f, 0.7f));
        float top = Top(desk);
        Transform monitor = Model(r, "LEDMonitor", new Vector3(0f, top, -0.18f), new Vector3(0.58f, 0.44f, 0.24f));
        MonitorScreen(monitor, 1f, out Vector3 at, out Vector3 size, out Vector3 euler);
        StaticMonitor(r, at, size, euler);
        Model(r, "Notepad", new Vector3(-0.45f, top, 0.05f), new Vector3(0.21f, 0.03f, 0.3f), 12f);
        Model(r, "OfficeChair", new Vector3(0.1f, 0f, 0.62f), new Vector3(0.65f, 1.2f, 0.65f), 194f);
        return r.gameObject;
    }

    static GameObject LaptopDeskModel()
    {
        Transform r = Root("LaptopDesk");
        Transform desk = Model(r, "OfficeDesk", Vector3.zero, new Vector3(1.2f, 0.76f, 0.6f));
        float top = Top(desk);
        
        Transform laptop = Model(r, "Laptop", new Vector3(0.1f, top, 0f), new Vector3(0.36f, 0.26f, 0.3f), 90f);
        Bounds b = ModelBounds(laptop);
        StaticMonitor(r, new Vector3(b.center.x, b.min.y + b.size.y * 0.56f, b.min.z + b.size.z * 0.14f),
            new Vector3(b.size.x * 0.84f, b.size.y * 0.66f, 0.003f), new Vector3(-14f, 0f, 0f));
        Model(r, "Notepad", new Vector3(-0.35f, top, 0.05f), new Vector3(0.21f, 0.03f, 0.3f), 15f);
        return r.gameObject;
    }

    static GameObject TradingPodModel()
    {
        Transform r = Root("TradingPod");
        Transform table = Model(r, "BoardroomTable", Vector3.zero, new Vector3(2.8f, 0.78f, 1.5f), keep: "long");
        float top = Top(table);
        Material[] screens = { tickerGreen, tickerRed, tickerBlue, tickerGreen, tickerRed, tickerGreen, tickerBlue, tickerRed };
        int s = 0;
        foreach (int side in new[] { -1, 1 })
        {
            
            for (int i = 0; i < 4; i++)
            {
                float x = -1.05f + i * 0.7f;
                Transform monitor = Model(r, "LEDMonitor", new Vector3(x, top, side * 0.12f), new Vector3(0.6f, 0.45f, 0.24f), side > 0 ? 0f : 180f);
                MonitorScreen(monitor, side, out Vector3 at, out Vector3 size, out Vector3 euler);
                Box(r, at, size, screens[s++ % screens.Length], euler);
            }
            foreach (float x in new[] { -0.7f, 0.7f })
                Model(r, "OfficeChair", new Vector3(x + side * 0.1f, 0f, side * 1.05f), new Vector3(0.65f, 1.2f, 0.65f), side > 0 ? 188f : 8f);
            Model(r, "Notepad", new Vector3(-1.1f, top, side * 0.5f), new Vector3(0.21f, 0.03f, 0.3f), 20f);
        }
        return r.gameObject;
    }

    static GameObject ExecutiveDeskModel()
    {
        Transform r = Root("ExecutiveDesk");
        
        Transform desk = Model(r, "WoodenTable", Vector3.zero, new Vector3(1.8f, 0.8f, 0.85f));
        float top = Top(desk);
        Model(r, "DeskLamp", new Vector3(-0.62f, top, -0.2f), new Vector3(0.42f, 0.45f, 0.3f));
        Transform laptop = Model(r, "Laptop", new Vector3(0.25f, top, -0.02f), new Vector3(0.36f, 0.26f, 0.3f), 90f);
        Bounds b = ModelBounds(laptop);
        Box(r, new Vector3(b.center.x, b.min.y + b.size.y * 0.56f, b.min.z + b.size.z * 0.14f),
            new Vector3(b.size.x * 0.84f, b.size.y * 0.66f, 0.003f), tickerGreen, new Vector3(-14f, 0f, 0f));
        Model(r, "Notepad", new Vector3(-0.15f, top, 0.18f), new Vector3(0.21f, 0.03f, 0.3f), -12f);
        Model(r, "LeatherChair", new Vector3(0f, 0f, 0.8f), new Vector3(0.7f, 1.2f, 0.8f), 180f);
        return r.gameObject;
    }

    static GameObject BoardroomModel()
    {
        Transform r = Root("Boardroom");
        
        Transform table = Model(r, "MeetingTable", Vector3.zero, new Vector3(4.6f, 0.78f, 1.4f), 90f);
        float top = Top(table);
        var chair = new Vector3(0.7f, 1.2f, 0.8f);
        for (int i = 0; i < 4; i++)
        {
            float x = -1.5f + i * 1f;
            Model(r, "LeatherChair", new Vector3(x, 0f, 1.05f), chair, 180f);
            Model(r, "LeatherChair", new Vector3(x + 0.15f, 0f, -1.05f), chair, 0f);
            Model(r, "Notepad", new Vector3(x, top, 0.35f), new Vector3(0.21f, 0.03f, 0.3f), i * 9f);
        }
        Model(r, "LeatherChair", new Vector3(-2.65f, 0f, 0f), chair, 90f);
        Model(r, "LeatherChair", new Vector3(2.65f, 0f, 0f), chair, -90f);
        return r.gameObject;
    }

    
    static bool CureBriefcaseModel(Transform briefcase)
    {
        if (!PropModels.Has("Briefcase")) return false;
        Model(briefcase, "Briefcase", Vector3.zero, new Vector3(0.5f, 0.4f, 0.16f));
        Bounds b = ModelBounds(briefcase.GetChild(briefcase.childCount - 1));
        Box(briefcase, new Vector3(0f, b.center.y - 0.04f, b.max.z + 0.002f), new Vector3(0.1f, 0.1f, 0.004f), cureGlow, new Vector3(0f, 0f, 45f));
        return true;
    }

    
    
    public static void RebuildModelProps()
    {
        
        EnsureAssets();
        overwrite = true;
        try
        {
            BuildOffice();
            BuildTrading();
            BuildArchive();
            BuildHospital();
            BuildFurnace();
            SavePrefab(CureVault, $"{LabDir}/CureVault.prefab", PrefabKind.Decor);
            AssetDatabase.SaveAssets();
            FixEmissionKeywords();
        }
        finally { overwrite = false; }
    }

    
    public static IEnumerable<string> ModelPrefabPaths()
    {
        foreach (var kv in ModelBuilders) if (PropModels.Has(kv.Value.model)) yield return kv.Key;
        yield return $"{LabDir}/CureVault.prefab";
    }
}

