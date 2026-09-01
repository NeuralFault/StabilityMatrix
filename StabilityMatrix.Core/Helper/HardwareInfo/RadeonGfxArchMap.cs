namespace StabilityMatrix.Core.Helper.HardwareInfo;

/// <summary>
/// Single source of truth for mapping AMD Radeon hardware to gfx IP architecture strings.
/// PCI device IDs (from <c>lspci -nn</c>) are authoritative; marketing-name substring matching
/// is used as a fallback when device IDs are unavailable (e.g. Windows WMI enumeration).
/// IDs for new AMD GPUs can be searched at <c>https://pci-ids.ucw.cz/read/PC/1002</c>
/// Matching GFX architectures with search on TechPowerUP GPU Database at <c>https://www.techpowerup.com/gpu-specs/</c>.
/// </summary>
internal static class RadeonGfxArchMap
{
    private static readonly IReadOnlyDictionary<string, string> DeviceIdMap = new Dictionary<string, string>
    {
        // Vega 10 (GCN 5.0) - gfx900
        ["6860"] = "gfx900", // Instinct MI25 / MI25x2 / V340
        ["6861"] = "gfx900", // Radeon PRO WX 9100
        ["6863"] = "gfx900", // Vega Frontier Edition
        ["6867"] = "gfx900", // Radeon Pro Vega 56
        ["687f"] = "gfx900", // RX Vega 56 / 64

        // Vega 20 (GCN 5.1) - gfx906
        ["66a1"] = "gfx906", // Radeon Pro VII / Instinct MI50
        ["66a3"] = "gfx906", // Radeon Pro Vega II / Duo
        ["66af"] = "gfx906", // Radeon VII

        // CDNA 1 - gfx908
        ["7388"] = "gfx908",
        ["738c"] = "gfx908",
        ["738e"] = "gfx908", // Instinct MI100 PCIe

        // CDNA 2 - gfx90a
        ["740f"] = "gfx90a", // Instinct MI210 PCIe

        // CDNA 3 - gfx950
        ["75a8"] = "gfx950", // Instinct MI350P PCIe

        // Navi 10 (RDNA 1) - gfx1010
        ["7310"] = "gfx1010", // Radeon Pro W5700X
        ["7312"] = "gfx1010", // Radeon Pro W5700
        ["731f"] = "gfx1010", // RX 5600 XT / 5700 / 5700 XT

        // Navi 12 (RDNA 1) - gfx1011
        ["7360"] = "gfx1011", // Radeon Pro 5600M / V520

        // Navi 14 (RDNA 1) - gfx1012
        ["7340"] = "gfx1012", // RX 5500 / 5500M
        ["7341"] = "gfx1012", // Radeon Pro W5500

        // Navi 21 (RDNA 2) - gfx1030
        ["73a1"] = "gfx1030", // Radeon PRO V620
        ["73a3"] = "gfx1030", // Radeon PRO W6800
        ["73a5"] = "gfx1030", // RX 6950 XT
        ["73ae"] = "gfx1030", // Radeon PRO V620 MxGPU
        ["73af"] = "gfx1030", // RX 6900 XT
        ["73bf"] = "gfx1030", // RX 6800 / 6800 XT / 6900 XT

        // Navi 22 (RDNA 2) - gfx1031
        ["73df"] = "gfx1031", // RX 6700 / 6700 XT / 6750 XT / 6800M / 6850M XT

        // Navi 23 (RDNA 2) - gfx1032
        ["73ef"] = "gfx1032", // RX 6650 XT / 6700S / 6800S
        ["73ff"] = "gfx1032", // RX 6600 / 6600 XT / 6600M

        // Van Gogh APU (Steam Deck) - gfx1033
        ["1435"] = "gfx1033", // Steam Deck (Van Gogh / Aerith / Sephiroth)

        // Navi 24 (RDNA 2) - gfx1034
        ["7422"] = "gfx1034", // Radeon PRO W6400
        ["7424"] = "gfx1034", // RX 6300
        ["743f"] = "gfx1034", // RX 6400 / 6500 XT / 6500M

        // Rembrandt APU - gfx1035
        ["1681"] = "gfx1035", // Radeon 680M / 660M

        // Navi 31 (RDNA 3) - gfx1100
        ["7448"] = "gfx1100", // Radeon Pro W7900
        ["7449"] = "gfx1100", // Radeon PRO W7800 (48 GB)
        ["744c"] = "gfx1100", // RX 7900 XT / XTX / GRE
        ["745e"] = "gfx1100", // Radeon PRO W7800

        // Navi 32 (RDNA 3) -  gfx1101
        ["7460"] = "gfx1101", // Radeon PRO V710
        ["7461"] = "gfx1101", // Radeon PRO V710 MxGPU
        ["7470"] = "gfx1101", // Radeon PRO W7700
        ["747e"] = "gfx1101", // RX 7700 XT / 7800 XT

        // Navi 33 (RDNA 3) - gfx1102
        ["7480"] = "gfx1102", // RX 7600 / 7600 XT / 7650 GRE / 7700S
        ["7499"] = "gfx1102", // RX 7400
        ["749f"] = "gfx1102", // RX 7500

        // Phoenix Point / Hawk Point APUs - gfx1103
        ["15bf"] = "gfx1103", // Radeon 780M / 760M / 740M
        ["15c8"] = "gfx1103", // Radeon 740M (Phoenix2)
        ["1900"] = "gfx1103", // Hawk Point / Ryzen Z2

        // Strix Point APU - gfx1150
        ["150e"] = "gfx1150", // Radeon 880M / 890M / Ryzen Z2 Extreme

        // Strix Halo APU - gfx1151
        ["1586"] = "gfx1151", // Radeon 8040S / 8050S / 8060S

        // Krackan Point APU - gfx1152
        ["1114"] = "gfx1152", // Radeon 840M / 860M

        // Navi 44 (RDNA 4) - gfx1200
        ["7590"] = "gfx1200", // RX 9050 / 9060 XT

        // Navi 48 (RDNA 4) - gfx1201
        ["7550"] = "gfx1201", // RX 9070 / 9070 XT / 9070 GRE
        ["7551"] = "gfx1201", // Radeon AI PRO R9700 / Radeon AI PRO R9700S / Radeon AI Pro R9600D
    };

    public static string? GetGfxArchFromDeviceId(string? deviceId)
    {
        return string.IsNullOrWhiteSpace(deviceId)
            ? null
            : DeviceIdMap.GetValueOrDefault(deviceId.ToLowerInvariant());
    }

    public static string? GetGfxArchFromName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        // Normalize for safer substring checks (handles RX7800 vs RX 7800, etc.)
        // Order matters: first match wins. keep specific substrings (e.g. "5600M", "7700S") above shorter prefixes (e.g. "5600", "7700").
        var nameNoSpaces = name.Replace(" ", "", StringComparison.Ordinal);

        return name switch
        {
            // RDNA4
            _ when Has("R9700") || Has("R9600") || Has("9070") => "gfx1201",
            _ when Has("9060") || Has("9050") => "gfx1200",

            // RDNA3.5 APUs
            _ when Has("840M") || Has("860M") => "gfx1152",
            _ when Has("880M") || Has("890M") || Has("Z2 Extreme") => "gfx1150",
            _ when Has("8040S") || Has("8050S") || Has("8060S") => "gfx1151",

            // RDNA3 APUs (Phoenix)
            _ when Has("740M") || Has("760M") || Has("780M") || Has("Z1") || Has("Z2") => "gfx1103",

            // RDNA3 dGPU Navi33
            _ when Has("7400") || Has("7500") || Has("7600") || Has("7650") || Has("7700S") => "gfx1102",

            // RDNA3 dGPU Navi32
            _ when Has("7700") || Has("RX 7800") || Has("v710") || HasNoSpace("RX7800") => "gfx1101",

            // RDNA3 dGPU Navi31 (incl. Pro)
            _ when Has("W7800") || Has("7900") => "gfx1100",

            // RDNA2 APUs (Rembrandt)
            _ when Has("660M") || Has("680M") => "gfx1035",

            // RDNA2 Navi24 low-end (incl. some mobiles)
            _ when Has("6300") || Has("6400") || Has("6450") || Has("6500") || Has("6550") || Has("6500M") =>
                "gfx1034",

            // RDNA2 Steam Deck APU
            _ when Has("Van Gogh") || Has("Sephiroth") || Has("Custom GPU") => "gfx1033",

            // RDNA2 Navi23
            _ when Has("6600") || Has("6650") || Has("6700S") || Has("6800S") || Has("6600M") => "gfx1032",

            // RDNA2 Navi22 (note: desktop 6800 is NOT here; that's Navi21/gfx1030)
            _ when Has("6700") || Has("6750") || Has("6800M") || Has("6850M") => "gfx1031",

            // RDNA2 Navi21 (big die)
            _ when Has("6800") || Has("6900") || Has("6950") || Has("v620") => "gfx1030",

            // RDNA1 Navi10 XTX
            _ when Has("5500") => "gfx1012",

            // RDNA1 Pro Card
            _ when Has("v520") || Has("5600M") => "gfx1011",

            // RDNA1 Navi10 XT
            _ when Has("5600") || Has("5700") => "gfx1010",

            // Vega/GCN5 Dedicated GPUs
            _
                when Has("rx vega")
                    || Has("vega 64")
                    || Has("vega 56")
                    || Has("vega frontier")
                    || Has("WX 9100")
                    || (Has("MI25") && !Has("MI250"))
                    || Has("V340") => "gfx900",
            _
                when Has("radeon vii")
                    || HasNoSpace("radeonvii")
                    || Has("pro vii")
                    || HasNoSpace("provii")
                    || Has("vega ii")
                    || Has("MI50")
                    || Has("MI60") => "gfx906",

            // CDNA 1 (Instinct MI100)
            _ when Has("MI100") => "gfx908",

            // CDNA 2 (Instinct MI210)
            _ when Has("MI210") => "gfx90a",

            // CDNA 3 (Instinct MI350)
            _ when Has("MI350") => "gfx950",
            _ => null,
        };

        bool HasNoSpace(string s) =>
            nameNoSpaces.Contains(
                s.Replace(" ", "", StringComparison.Ordinal),
                StringComparison.OrdinalIgnoreCase
            );
        bool Has(string s) => name.Contains(s, StringComparison.OrdinalIgnoreCase);
    }
}
