window.XLBENCH_DATA = {
  "updated": "2026-07-30 14:03:24Z",
  "versions": {
    "ClosedXML": "0.105.0",
    "EPPlus": "8.6.3",
    "OpenXML SDK": "3.5.1",
    "NPOI": "2.8.0",
    "MiniExcel": "1.45.0",
    "XLibur": "0.106.1-beta.80"
  },
  "scenarios": [
    {
      "key": "OpenWorkbook",
      "label": "Read \u00B7 open workbook",
      "libraries": [
        "NPOI",
        "XLibur",
        "EPPlus",
        "ClosedXML"
      ],
      "timeMs": [
        247.07,
        1409.39,
        1730.75,
        3636.24
      ],
      "allocMb": [
        211.37,
        158.39,
        1038.89,
        1306.4
      ],
      "errorMs": [
        4.9,
        25.21,
        27.01,
        72.39
      ],
      "stdDevMs": [
        10.224,
        23.584,
        23.939,
        67.716
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
        2347.77,
        2379.55,
        2403.39,
        4168.7,
        5114.04,
        20183.22
      ],
      "allocMb": [
        1853.58,
        1255.32,
        659.64,
        1350.26,
        2157.21,
        2177.89
      ],
      "errorMs": [
        46.9,
        40.28,
        47.9,
        79.12,
        43.19,
        324.65
      ],
      "stdDevMs": [
        46.062,
        35.703,
        75.979,
        91.115,
        38.291,
        287.797
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
        63.83,
        162.06,
        241.77,
        394.86,
        450.99,
        722.82
      ],
      "allocMb": [
        84.59,
        134.2,
        67.45,
        181.1,
        322.9,
        247.46
      ],
      "errorMs": [
        1.27,
        3.18,
        3.42,
        7.73,
        8.96,
        14.16
      ],
      "stdDevMs": [
        2.455,
        3.264,
        3.036,
        11.811,
        9.199,
        16.302
      ]
    },
    {
      "key": "CreateStockReport",
      "label": "Report \u00B7 data \u002B conditional formatting \u002B chart",
      "libraries": [
        "OpenXML SDK",
        "XLibur",
        "ClosedXML",
        "EPPlus",
        "NPOI"
      ],
      "timeMs": [
        1.84,
        2.46,
        4.02,
        5.4,
        6.88
      ],
      "allocMb": [
        1.26,
        1.3,
        1.99,
        8.4,
        4.41
      ],
      "errorMs": [
        0.04,
        0.04,
        0.07,
        0.1,
        0.13
      ],
      "stdDevMs": [
        0.036,
        0.074,
        0.172,
        0.244,
        0.171
      ]
    }
  ]
};
