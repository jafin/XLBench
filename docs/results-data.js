window.XLBENCH_DATA = {
  "updated": "2026-08-01 21:43:44Z",
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
        240.58,
        1355.57,
        1809.58,
        3696.3,
        4601.88
      ],
      "allocMb": [
        211.37,
        158.32,
        1038.89,
        1306.4,
        7235.8
      ],
      "errorMs": [
        6.69,
        19.59,
        154.67,
        336.21,
        189.32
      ],
      "stdDevMs": [
        3.5,
        8.7,
        102.307,
        222.381,
        125.225
      ]
    },
    {
      "key": "OpenAndReadAll",
      "label": "Read \u00B7 open \u002B read all cells",
      "libraries": [
        "XLibur",
        "EPPlus",
        "OpenXML SDK",
        "MiniExcel",
        "NPOI",
        "IronXL",
        "ClosedXML"
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
        2273.54,
        2399.58,
        2453.87,
        3926.54,
        5119.41,
        15636.72,
        19975.88
      ],
      "allocMb": [
        644.47,
        1853.58,
        1255.32,
        1350.26,
        2157.21,
        12475.61,
        2177.89
      ],
      "errorMs": [
        52.97,
        153.18,
        45.45,
        92.52,
        102.35,
        552.99,
        385.83
      ],
      "stdDevMs": [
        31.523,
        101.316,
        23.772,
        61.193,
        45.443,
        329.076,
        171.31
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
        60.68,
        165.94,
        237.01,
        398.3,
        437.09,
        683.42,
        919.85
      ],
      "allocMb": [
        84.59,
        134.2,
        60.52,
        181.09,
        322.9,
        247.46,
        797.63
      ],
      "errorMs": [
        3.57,
        3.26,
        10.41,
        16.17,
        13.03,
        29.09,
        24.63
      ],
      "stdDevMs": [
        2.36,
        1.162,
        6.885,
        10.694,
        8.617,
        19.241,
        14.655
      ]
    },
    {
      "key": "CreateStockReport",
      "label": "Report \u00B7 data \u002B conditional formatting \u002B chart",
      "libraries": [
        "OpenXML SDK",
        "XLibur",
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
        8.47,
        9.28,
        15.57,
        16.84,
        36.94,
        376.81
      ],
      "allocMb": [
        4.92,
        3.49,
        14,
        8.02,
        16.26,
        237.38
      ],
      "errorMs": [
        0.22,
        0.46,
        0.46,
        0.45,
        10.38,
        33.67
      ],
      "stdDevMs": [
        0.143,
        0.301,
        0.307,
        0.297,
        6.863,
        22.269
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
        26.14,
        96.72,
        172.91,
        350.47,
        396,
        1580.18
      ],
      "allocMb": [
        9.8,
        142.49,
        164.9,
        337.71,
        413.38,
        753.86
      ],
      "errorMs": [
        0.54,
        6.68,
        9.6,
        9,
        10.22,
        34.84
      ],
      "stdDevMs": [
        0.355,
        4.417,
        6.349,
        5.95,
        6.762,
        20.73
      ]
    }
  ]
};
