window.XLBENCH_DATA = {
  "updated": "2026-08-06 15:35:28Z",
  "versions": {
    "ClosedXML": "0.105.1",
    "EPPlus": "8.6.3",
    "OpenXML SDK": "3.5.1",
    "NPOI": "2.8.0",
    "MiniExcel": "1.45.0",
    "XLibur": "0.310.0",
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
        "IronXL"
      ],
      "snapshotOf": [
        null,
        null,
        null,
        null,
        "2026.8.1, Job-HTCPYF, captured 2026-08-03"
      ],
      "timeMs": [
        9.44,
        16.85,
        29.71,
        38.37,
        208.75
      ],
      "allocMb": [
        1.9,
        1.74,
        13.88,
        13.18,
        152.81
      ],
      "errorMs": [
        0.45,
        0.98,
        14.29,
        3.49,
        155.76
      ],
      "stdDevMs": [
        0.266,
        0.51,
        8.503,
        2.079,
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
        "2026.8.1, Job-HTCPYF, captured 2026-08-03"
      ],
      "timeMs": [
        694.31,
        1128.88,
        1220.58,
        1255.79,
        2618.66,
        6522.65,
        9318.11
      ],
      "allocMb": [
        629.69,
        308.54,
        925.23,
        628.84,
        1077.8,
        1083.14,
        6333.04
      ],
      "errorMs": [
        11.61,
        22.08,
        28.97,
        20.44,
        175.09,
        125.76,
        477.33
      ],
      "stdDevMs": [
        6.073,
        14.601,
        17.241,
        10.69,
        104.195,
        74.839,
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
        "2026.8.1, Job-HTCPYF, captured 2026-08-03"
      ],
      "timeMs": [
        61.12,
        173.23,
        246.23,
        414.64,
        461.4,
        722.67,
        943.14
      ],
      "allocMb": [
        84.59,
        134.2,
        60.51,
        181.09,
        322.9,
        247.46,
        797.63
      ],
      "errorMs": [
        1.99,
        7.79,
        13.31,
        12.16,
        15.15,
        14.16,
        18.52
      ],
      "stdDevMs": [
        1.186,
        5.155,
        8.801,
        8.04,
        10.022,
        6.288,
        11.02
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
        "2026.8.1, Job-HTCPYF, captured 2026-08-03"
      ],
      "timeMs": [
        8.59,
        9.99,
        19.64,
        22.18,
        41.6,
        387.85
      ],
      "allocMb": [
        4.92,
        3.48,
        14,
        8.02,
        16.26,
        237.38
      ],
      "errorMs": [
        0.37,
        0.62,
        6.01,
        4.74,
        16.42,
        36.55
      ],
      "stdDevMs": [
        0.223,
        0.41,
        3.978,
        3.136,
        10.861,
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
        12.71,
        25.43,
        100.83,
        401.24,
        404.61,
        1577.27
      ],
      "allocMb": [
        4.2,
        9.8,
        142.49,
        337.77,
        413.44,
        753.86
      ],
      "errorMs": [
        0.22,
        0.93,
        14.98,
        14.6,
        8.82,
        62.96
      ],
      "stdDevMs": [
        0.115,
        0.554,
        9.909,
        9.656,
        5.248,
        41.643
      ]
    },
    {
      "key": "InsertColumnsAndRecalculate",
      "label": "Edit \u00B7 insert 2 columns \u002B recalculate",
      "libraries": [
        "XLibur",
        "EPPlus",
        "OpenXML SDK",
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
        "2026.8.1, Job-HTCPYF, captured 2026-08-03"
      ],
      "timeMs": [
        15.3,
        17.53,
        36.23,
        43.33,
        57.57,
        113.28
      ],
      "allocMb": [
        4.87,
        11.41,
        13.05,
        13.64,
        30.72,
        104.88
      ],
      "errorMs": [
        0.54,
        0.32,
        1.12,
        17.34,
        19.89,
        41.97
      ],
      "stdDevMs": [
        0.356,
        0.169,
        0.742,
        11.467,
        13.154,
        27.764
      ]
    }
  ]
};
