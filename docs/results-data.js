window.XLBENCH_DATA = {
  "updated": "2026-09-26 13:23:17Z",
  "versions": {
    "ClosedXML": "0.105.1",
    "EPPlus": "8.7.1",
    "OpenXML SDK": "3.5.1",
    "NPOI": "2.8.1",
    "MiniExcel": "1.46.0",
    "XLibur": "0.620.0",
    "Telerik": "2026.3.923.100",
    "IronXL": "2026.8.1"
  },
  "snapshots": {
    "IronXL": "2026.8.1, Job-HTCPYF, captured 2026-08-03"
  },
  "scenarios": [
    {
      "key": "OpenAmendPropertiesAndSave",
      "label": "Read \u00B7 open \u002B set properties \u002B save",
      "libraries": [
        "NPOI",
        "XLibur",
        "EPPlus",
        "ClosedXML",
        "Telerik",
        "IronXL"
      ],
      "snapshotOf": [
        null,
        null,
        null,
        null,
        null,
        "2026.8.1, Job-HTCPYF, captured 2026-08-03"
      ],
      "timeMs": [
        8.77,
        15.56,
        24.3,
        33.93,
        83.92,
        208.75
      ],
      "allocMb": [
        1.89,
        1.73,
        13.81,
        13.18,
        185.61,
        152.81
      ],
      "errorMs": [
        0.17,
        0.31,
        0.48,
        0.66,
        1.57,
        155.76
      ],
      "stdDevMs": [
        0.238,
        0.327,
        0.592,
        0.585,
        1.612,
        92.688
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
        "Telerik",
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
        null,
        "2026.8.1, Job-HTCPYF, captured 2026-08-03"
      ],
      "timeMs": [
        762.2,
        929.79,
        1158.82,
        1197.99,
        2502.55,
        5741.21,
        6128.4,
        9318.11
      ],
      "allocMb": [
        806.68,
        201.35,
        627.82,
        925.21,
        1075.67,
        4136.75,
        1074.14,
        6333.04
      ],
      "errorMs": [
        11.3,
        18.47,
        17.89,
        22.11,
        45.25,
        111.07,
        66.52,
        477.33
      ],
      "stdDevMs": [
        10.569,
        20.534,
        15.857,
        56.285,
        42.328,
        109.084,
        62.225,
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
        "IronXL",
        "Telerik"
      ],
      "snapshotOf": [
        null,
        null,
        null,
        null,
        null,
        null,
        "2026.8.1, Job-HTCPYF, captured 2026-08-03",
        null
      ],
      "timeMs": [
        61.6,
        165.23,
        231.07,
        407.46,
        450.04,
        694.98,
        943.14,
        1787.88
      ],
      "allocMb": [
        84.59,
        134.19,
        60.5,
        181.09,
        322.83,
        246.16,
        797.63,
        2097.04
      ],
      "errorMs": [
        1.23,
        3.3,
        4.58,
        8.03,
        8.79,
        13.36,
        18.52,
        35.36
      ],
      "stdDevMs": [
        1.64,
        6.361,
        6.85,
        7.89,
        12.319,
        17.369,
        11.02,
        68.975
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
        "Telerik",
        "IronXL"
      ],
      "snapshotOf": [
        null,
        null,
        null,
        null,
        null,
        null,
        "2026.8.1, Job-HTCPYF, captured 2026-08-03"
      ],
      "timeMs": [
        8.15,
        9.37,
        15.31,
        16.69,
        32.18,
        147.18,
        387.85
      ],
      "allocMb": [
        4.92,
        3.35,
        13.92,
        8.02,
        16.43,
        432.4,
        237.38
      ],
      "errorMs": [
        0.14,
        0.18,
        0.3,
        0.27,
        0.64,
        3.55,
        36.55
      ],
      "stdDevMs": [
        0.137,
        0.326,
        0.468,
        0.249,
        1.055,
        10.023,
        24.173
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
        "IronXL",
        "Telerik"
      ],
      "snapshotOf": [
        null,
        null,
        null,
        null,
        null,
        "2026.8.1, Job-HTCPYF, captured 2026-08-03",
        null
      ],
      "timeMs": [
        11.69,
        25.83,
        89.9,
        361.73,
        431.55,
        1577.27,
        6324.93
      ],
      "allocMb": [
        3.63,
        9.85,
        142.49,
        340.72,
        413.44,
        753.86,
        16843.7
      ],
      "errorMs": [
        0.15,
        0.52,
        1.7,
        7.21,
        8.52,
        62.96,
        125.02
      ],
      "stdDevMs": [
        0.133,
        1.134,
        2.267,
        6.017,
        11.937,
        41.643,
        166.893
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
        "IronXL",
        "Telerik"
      ],
      "snapshotOf": [
        null,
        null,
        null,
        null,
        null,
        "2026.8.1, Job-HTCPYF, captured 2026-08-03",
        null
      ],
      "timeMs": [
        13.35,
        16.16,
        26.56,
        33.81,
        37.27,
        113.28,
        188.84
      ],
      "allocMb": [
        3.99,
        11.41,
        13.68,
        13.05,
        30.72,
        104.88,
        280.01
      ],
      "errorMs": [
        0.25,
        0.3,
        0.52,
        0.64,
        0.74,
        41.97,
        3.75
      ],
      "stdDevMs": [
        0.496,
        0.717,
        0.614,
        0.706,
        1.855,
        27.764,
        10.341
      ]
    }
  ]
};
