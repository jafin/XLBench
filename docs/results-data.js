window.XLBENCH_DATA = {
  "updated": "2026-08-03 20:07:01Z",
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
      "key": "OpenAmendPropertiesAndSave",
      "label": "Read \u00B7 open \u002B set properties \u002B save",
      "libraries": [
        "NPOI",
        "EPPlus",
        "XLibur",
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
        8.98,
        28.51,
        29.34,
        37.76,
        208.75
      ],
      "allocMb": [
        1.9,
        13.88,
        6.72,
        13.2,
        152.81
      ],
      "errorMs": [
        0.35,
        6.45,
        10.04,
        4.07,
        155.76
      ],
      "stdDevMs": [
        0.205,
        4.268,
        5.251,
        2.423,
        92.688
      ]
    },
    {
      "key": "OpenAndReadAll",
      "label": "Read \u00B7 open \u002B read all cells",
      "libraries": [
        "MiniExcel",
        "XLibur",
        "EPPlus",
        "OpenXML SDK",
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
        703.48,
        1246.04,
        1267.07,
        1285.49,
        2827.3,
        6414.42,
        9318.11
      ],
      "allocMb": [
        629.71,
        320.43,
        925.23,
        628.84,
        1077.8,
        1083.78,
        6333.04
      ],
      "errorMs": [
        29.64,
        48.38,
        67.44,
        36.2,
        255.34,
        209.89,
        477.33
      ],
      "stdDevMs": [
        19.604,
        32,
        44.608,
        23.941,
        168.894,
        124.902,
        315.727
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
        62.05,
        161.13,
        241.46,
        413.96,
        453.57,
        690.03,
        943.14
      ],
      "allocMb": [
        84.59,
        134.2,
        60.52,
        181.1,
        322.9,
        247.52,
        797.63
      ],
      "errorMs": [
        3.43,
        4.38,
        9.05,
        12.88,
        17.15,
        28.82,
        18.52
      ],
      "stdDevMs": [
        2.27,
        2.899,
        4.732,
        8.519,
        11.341,
        17.151,
        11.02
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
        9.1,
        10.34,
        19.15,
        26.81,
        34.35,
        387.85
      ],
      "allocMb": [
        4.92,
        3.49,
        8.02,
        14.15,
        16.25,
        237.38
      ],
      "errorMs": [
        1.91,
        1.02,
        1.75,
        11.74,
        2.41,
        36.55
      ],
      "stdDevMs": [
        1.263,
        0.678,
        1.042,
        7.767,
        1.258,
        24.173
      ]
    },
    {
      "key": "EditAndRecalculate",
      "label": "Edit \u00B7 delete rows \u002B set column \u002B recalculate",
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
        25.92,
        26.69,
        89.72,
        349.92,
        421.19,
        1577.27
      ],
      "allocMb": [
        9.83,
        4.39,
        142.49,
        337.77,
        413.44,
        753.86
      ],
      "errorMs": [
        0.62,
        15.97,
        2.69,
        23.9,
        35.19,
        62.96
      ],
      "stdDevMs": [
        0.409,
        10.566,
        1.601,
        14.223,
        20.939,
        41.643
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
        22,
        25.12,
        32.91,
        36.07,
        50.36,
        113.28
      ],
      "allocMb": [
        5.07,
        11.42,
        13.65,
        13.11,
        30.79,
        104.88
      ],
      "errorMs": [
        14.47,
        12.11,
        6.73,
        1.3,
        14.53,
        41.97
      ],
      "stdDevMs": [
        9.569,
        8.011,
        3.521,
        0.772,
        8.649,
        27.764
      ]
    }
  ]
};
