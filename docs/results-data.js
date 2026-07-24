window.XLBENCH_DATA = {
  "updated": "2026-07-24 10:57:07Z",
  "scenarios": [
    {
      "key": "OpenWorkbook",
      "label": "Read \u00B7 open workbook",
      "libraries": [
        "NPOI",
        "EPPlus",
        "XLibur",
        "ClosedXML"
      ],
      "timeMs": [
        514.05,
        3543.69,
        5313.04,
        7342.75
      ],
      "allocMb": [
        426.24,
        2086.15,
        1893.03,
        2609.62
      ]
    },
    {
      "key": "OpenAndReadAll",
      "label": "Read \u00B7 open \u002B read all cells",
      "libraries": [
        "EPPlus",
        "OpenXML SDK",
        "XLibur",
        "MiniExcel",
        "NPOI",
        "ClosedXML"
      ],
      "timeMs": [
        4631.7,
        4879.26,
        6874.35,
        7230.74,
        11383.47,
        72046.88
      ],
      "allocMb": [
        3727.63,
        2506.41,
        2879.36,
        2710.44,
        4321.42,
        4352.77
      ]
    },
    {
      "key": "CreateAndSave",
      "label": "Write \u00B7 create \u002B save",
      "libraries": [
        "MiniExcel",
        "OpenXML SDK",
        "XLibur",
        "ClosedXML",
        "EPPlus",
        "NPOI"
      ],
      "timeMs": [
        62.92,
        161.43,
        277.32,
        418.08,
        441.96,
        683.63
      ],
      "allocMb": [
        84.59,
        134.19,
        131.12,
        181.09,
        322.83,
        247.27
      ]
    }
  ]
};
