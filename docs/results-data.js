window.XLBENCH_DATA = {
  "updated": "2026-07-24 12:08:55Z",
  "versions": {
    "ClosedXML": "0.105.0",
    "EPPlus": "8.6.2",
    "OpenXML SDK": "3.5.1",
    "NPOI": "2.8.0",
    "MiniExcel": "1.45.0",
    "XLibur": "0.105.1-rc.137"
  },
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
        635.99,
        3532.64,
        5116.98,
        7222.28
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
        4606.2,
        4900.24,
        6850.76,
        7264.94,
        11300.18,
        69839.84
      ],
      "allocMb": [
        3727.63,
        2506.41,
        2879.36,
        2710.44,
        4321.42,
        4350.23
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
        62.09,
        158.58,
        275.83,
        410.92,
        441.92,
        668.29
      ],
      "allocMb": [
        84.59,
        134.19,
        131.12,
        181.1,
        322.83,
        247.27
      ]
    }
  ]
};
