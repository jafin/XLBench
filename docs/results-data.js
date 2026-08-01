window.XLBENCH_DATA = {
  "updated": "2026-08-01 22:03:40Z",
  "versions": {
    "ClosedXML": "0.105.1",
    "EPPlus": "8.6.3",
    "OpenXML SDK": "3.5.1",
    "NPOI": "2.8.0",
    "MiniExcel": "1.45.0",
    "XLibur": "0.200.0",
    "IronXL": "2026.8.1"
  },
  "snapshots": {},
  "scenarios": [
    {
      "key": "OpenWorkbook",
      "label": "Read \u00B7 open workbook",
      "libraries": [
        "NPOI",
        "XLibur",
        "EPPlus",
        "ClosedXML",
        "IronXL"
      ],
      "snapshotOf": [
        null,
        null,
        null,
        null,
        null
      ],
      "timeMs": [
        115.35,
        667.06,
        850.51,
        1695.84,
        2201.53
      ],
      "allocMb": [
        105.92,
        80.39,
        518.21,
        654.05,
        3717.76
      ],
      "errorMs": [
        7.01,
        16.34,
        21.92,
        27.05,
        37.57
      ],
      "stdDevMs": [
        4.634,
        10.805,
        13.045,
        9.648,
        16.682
      ]
    },
    {
      "key": "OpenAndReadAll",
      "label": "Read \u00B7 open \u002B read all cells",
      "libraries": [
        "MiniExcel",
        "OpenXML SDK",
        "EPPlus",
        "XLibur",
        "NPOI",
        "ClosedXML",
        "IronXL"
      ],
      "snapshotOf": [
        null,
        null,
        null,
        null,
        null,
        null,
        null
      ],
      "timeMs": [
        671.28,
        1129.47,
        1156.47,
        1160.99,
        2545.35,
        6184.32,
        7788.2
      ],
      "allocMb": [
        629.7,
        628.84,
        925.23,
        320.05,
        1077.8,
        1083.78,
        6333.03
      ],
      "errorMs": [
        15.04,
        17.06,
        21.88,
        60.56,
        59.95,
        91.14,
        256.73
      ],
      "stdDevMs": [
        9.946,
        6.083,
        13.022,
        40.056,
        31.353,
        40.468,
        169.812
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
        "NPOI",
        "IronXL"
      ],
      "snapshotOf": [
        null,
        null,
        null,
        null,
        null,
        null,
        null
      ],
      "timeMs": [
        60.57,
        159.81,
        240.58,
        403.99,
        450.61,
        671.24,
        920.66
      ],
      "allocMb": [
        84.59,
        134.2,
        60.52,
        181.1,
        322.9,
        247.46,
        797.63
      ],
      "errorMs": [
        2.59,
        5.07,
        6.66,
        22.11,
        24.31,
        20.49,
        65.94
      ],
      "stdDevMs": [
        1.713,
        3.352,
        4.402,
        13.159,
        16.08,
        13.552,
        43.613
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
        "NPOI",
        "IronXL"
      ],
      "snapshotOf": [
        null,
        null,
        null,
        null,
        null,
        null
      ],
      "timeMs": [
        8.75,
        10.23,
        17,
        32.48,
        32.73,
        374.58
      ],
      "allocMb": [
        4.92,
        3.49,
        8.02,
        14.14,
        16.27,
        237.38
      ],
      "errorMs": [
        0.57,
        0.66,
        0.49,
        14.32,
        2,
        38.14
      ],
      "stdDevMs": [
        0.374,
        0.434,
        0.326,
        9.471,
        1.048,
        19.949
      ]
    },
    {
      "key": "EditAndRecalculate",
      "label": "Edit \u00B7 delete rows \u002B set column \u002B recalculate",
      "libraries": [
        "OpenXML SDK",
        "EPPlus",
        "XLibur",
        "ClosedXML",
        "NPOI",
        "IronXL"
      ],
      "snapshotOf": [
        null,
        null,
        null,
        null,
        null,
        null
      ],
      "timeMs": [
        26.26,
        90.99,
        172.7,
        351.02,
        405.05,
        1562.62
      ],
      "allocMb": [
        9.83,
        142.49,
        164.91,
        337.93,
        413.44,
        753.86
      ],
      "errorMs": [
        0.54,
        3.2,
        3.25,
        16.94,
        7.36,
        46.52
      ],
      "stdDevMs": [
        0.358,
        2.12,
        2.152,
        10.078,
        4.867,
        27.684
      ]
    }
  ]
};
