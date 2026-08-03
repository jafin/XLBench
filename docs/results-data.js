window.XLBENCH_DATA = {
  "updated": "2026-08-03 18:18:41Z",
  "versions": {
    "ClosedXML": "0.105.1",
    "EPPlus": "8.6.3",
    "OpenXML SDK": "3.5.1",
    "NPOI": "2.8.0",
    "MiniExcel": "1.45.0",
    "XLibur": "0.300.0",
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
        155.53,
        694.03,
        941.47,
        1806.28,
        2572.06
      ],
      "allocMb": [
        105.96,
        80.77,
        518.2,
        654.05,
        3717.76
      ],
      "errorMs": [
        29.67,
        13.7,
        45.5,
        85.64,
        214.24
      ],
      "stdDevMs": [
        19.628,
        6.082,
        30.092,
        56.646,
        127.49
      ]
    },
    {
      "key": "OpenAndReadAll",
      "label": "Read \u00B7 open \u002B read all cells",
      "libraries": [
        "MiniExcel",
        "XLibur",
        "OpenXML SDK",
        "EPPlus",
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
        658.4,
        1179.3,
        1199.73,
        1278.45,
        2622.15,
        6765.29,
        8138.64
      ],
      "allocMb": [
        629.7,
        320.43,
        628.84,
        925.23,
        1077.8,
        1083.78,
        6333.03
      ],
      "errorMs": [
        18.41,
        30.12,
        73.58,
        101.56,
        136.09,
        196.02,
        391
      ],
      "stdDevMs": [
        10.954,
        19.922,
        48.669,
        67.177,
        71.178,
        129.658,
        258.62
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
        63.42,
        163.96,
        246.68,
        416.33,
        461.04,
        709.98,
        946.68
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
        2.88,
        6.68,
        8.37,
        24.35,
        14.59,
        23.81,
        7.85
      ],
      "stdDevMs": [
        1.903,
        3.972,
        5.538,
        16.109,
        9.652,
        15.749,
        1.215
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
        8.52,
        10,
        17.26,
        21.67,
        36.09,
        400.99
      ],
      "allocMb": [
        4.92,
        3.49,
        8.01,
        14,
        16.3,
        238.13
      ],
      "errorMs": [
        0.38,
        0.37,
        0.43,
        9.01,
        2.89,
        55.46
      ],
      "stdDevMs": [
        0.226,
        0.242,
        0.283,
        5.957,
        1.512,
        36.68
      ]
    },
    {
      "key": "EditAndRecalculate",
      "label": "Edit \u00B7 delete rows \u002B set column \u002B recalculate",
      "libraries": [
        "XLibur",
        "OpenXML SDK",
        "EPPlus",
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
        22.18,
        26.67,
        94.02,
        364.7,
        420.91,
        1639.73
      ],
      "allocMb": [
        4.39,
        9.83,
        142.49,
        340.72,
        413.44,
        753.86
      ],
      "errorMs": [
        13.46,
        1.12,
        7.55,
        26.55,
        20.35,
        52.26
      ],
      "stdDevMs": [
        8.905,
        0.741,
        4.49,
        17.56,
        13.458,
        31.101
      ]
    },
    {
      "key": "InsertColumnsAndRecalculate",
      "label": "Edit \u00B7 insert 2 columns \u002B recalculate",
      "libraries": [
        "XLibur",
        "EPPlus",
        "ClosedXML",
        "OpenXML SDK",
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
        15.47,
        17.28,
        36.41,
        37.99,
        47.98,
        101.56
      ],
      "allocMb": [
        5.06,
        11.41,
        13.66,
        13.11,
        30.77,
        104.88
      ],
      "errorMs": [
        0.74,
        0.52,
        8.41,
        2.86,
        11.4,
        6.12
      ],
      "stdDevMs": [
        0.49,
        0.274,
        5.005,
        1.89,
        6.784,
        3.203
      ]
    }
  ]
};
