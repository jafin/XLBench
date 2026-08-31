window.XLBENCH_DATA = {
  "updated": "2026-08-31 20:05:23Z",
  "versions": {
    "ClosedXML": "0.105.1",
    "EPPlus": "8.7.0",
    "OpenXML SDK": "3.5.1",
    "NPOI": "2.8.0",
    "MiniExcel": "1.46.0",
    "XLibur": "0.311.2-alpha.34",
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
        8.49,
        16.13,
        22.4,
        33.15,
        208.75
      ],
      "allocMb": [
        1.89,
        1.72,
        13.82,
        13.18,
        152.81
      ],
      "errorMs": [
        0.14,
        1.31,
        0.43,
        0.69,
        155.76
      ],
      "stdDevMs": [
        0.022,
        1.227,
        0.227,
        0.613,
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
        689.73,
        901.48,
        1110.74,
        1160.32,
        2376.65,
        5956.13,
        9318.11
      ],
      "allocMb": [
        806.7,
        189.9,
        925.22,
        627.82,
        1077.67,
        1074.77,
        6333.04
      ],
      "errorMs": [
        12.9,
        21.33,
        20.97,
        19.01,
        41.38,
        80.87,
        477.33
      ],
      "stdDevMs": [
        9.327,
        19.947,
        9.309,
        9.944,
        21.642,
        28.838,
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
        59.54,
        157.65,
        217.81,
        374.93,
        418.94,
        639.48,
        943.14
      ],
      "allocMb": [
        84.59,
        134.19,
        60.5,
        181.09,
        322.83,
        247.27,
        797.63
      ],
      "errorMs": [
        1.98,
        2.83,
        6.25,
        7.2,
        7.49,
        12.27,
        18.52
      ],
      "stdDevMs": [
        1.758,
        0.734,
        5.544,
        6.737,
        5.85,
        8.118,
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
        7.83,
        8.57,
        14.16,
        16.47,
        31.64,
        387.85
      ],
      "allocMb": [
        4.92,
        3.48,
        13.92,
        8.02,
        16.23,
        237.38
      ],
      "errorMs": [
        0.15,
        0.13,
        0.5,
        0.57,
        0.82,
        36.55
      ],
      "stdDevMs": [
        0.038,
        0.057,
        0.47,
        0.508,
        0.681,
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
        10.95,
        23.64,
        82.59,
        334.91,
        378.28,
        1577.27
      ],
      "allocMb": [
        4.11,
        9.8,
        142.49,
        337.66,
        413.38,
        753.86
      ],
      "errorMs": [
        0.19,
        0.46,
        1.63,
        2.83,
        7.56,
        62.96
      ],
      "stdDevMs": [
        0.098,
        0.119,
        1.523,
        0.438,
        4.498,
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
        "2026.8.1, Job-HTCPYF, captured 2026-08-03"
      ],
      "timeMs": [
        12.98,
        16.17,
        25.4,
        31.87,
        35.61,
        113.28
      ],
      "allocMb": [
        4.74,
        11.41,
        13.65,
        13.05,
        30.72,
        104.88
      ],
      "errorMs": [
        0.24,
        0.74,
        0.79,
        0.5,
        1.63,
        41.97
      ],
      "stdDevMs": [
        0.17,
        0.617,
        0.703,
        0.264,
        1.527,
        27.764
      ]
    }
  ]
};
