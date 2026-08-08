window.XLBENCH_DATA = {
  "updated": "2026-08-08 12:19:36Z",
  "versions": {
    "ClosedXML": "0.105.1",
    "EPPlus": "8.6.3",
    "OpenXML SDK": "3.5.1",
    "NPOI": "2.8.0",
    "MiniExcel": "1.45.0",
    "XLibur": "0.310.1-beta.5",
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
        9.03,
        15.91,
        24.6,
        35.98,
        208.75
      ],
      "allocMb": [
        1.89,
        1.73,
        13.81,
        13.18,
        152.81
      ],
      "errorMs": [
        0.13,
        0.33,
        0.48,
        0.68,
        155.76
      ],
      "stdDevMs": [
        0.117,
        0.959,
        0.705,
        0.913,
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
        744.26,
        963.77,
        1226.32,
        1291.24,
        2624.59,
        6410.03,
        9318.11
      ],
      "allocMb": [
        629.69,
        193.72,
        925.22,
        627.82,
        1077.67,
        1074.77,
        6333.04
      ],
      "errorMs": [
        13.16,
        17.06,
        23.65,
        25.11,
        34.29,
        97.55,
        477.33
      ],
      "stdDevMs": [
        19.287,
        14.242,
        31.57,
        27.905,
        35.218,
        81.457,
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
        63.03,
        173.62,
        260.47,
        410.59,
        443.15,
        675.8,
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
        1.17,
        3.44,
        6.04,
        8.17,
        8.4,
        13.22,
        18.52
      ],
      "stdDevMs": [
        1.604,
        8.753,
        17.439,
        14.945,
        7.861,
        16.234,
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
        8.18,
        9.52,
        15.34,
        17.61,
        32.26,
        387.85
      ],
      "allocMb": [
        4.92,
        3.44,
        13.92,
        8.02,
        16.23,
        237.38
      ],
      "errorMs": [
        0.09,
        0.16,
        0.29,
        0.28,
        0.59,
        36.55
      ],
      "stdDevMs": [
        0.073,
        0.225,
        0.418,
        0.251,
        0.75,
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
        12.34,
        27.17,
        95.69,
        356.95,
        418.78,
        1577.27
      ],
      "allocMb": [
        4.17,
        9.8,
        142.49,
        340.69,
        413.38,
        753.86
      ],
      "errorMs": [
        0.24,
        0.54,
        1.91,
        7.01,
        8.29,
        62.96
      ],
      "stdDevMs": [
        0.511,
        0.962,
        3.853,
        11.321,
        13.85,
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
        14.12,
        17.75,
        27.23,
        34.11,
        39.13,
        113.28
      ],
      "allocMb": [
        4.73,
        11.41,
        14.03,
        13.05,
        30.72,
        104.88
      ],
      "errorMs": [
        0.28,
        0.35,
        0.52,
        0.68,
        0.73,
        41.97
      ],
      "stdDevMs": [
        0.472,
        0.712,
        1.123,
        1.288,
        1.143,
        27.764
      ]
    }
  ]
};
