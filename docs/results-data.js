window.XLBENCH_DATA = {
  "updated": "2026-08-08 11:51:26Z",
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
        "IronXL"
      ],
      "snapshotOf": [
        "2026.8.1, Job-HTCPYF, captured 2026-08-03"
      ],
      "timeMs": [
        208.75
      ],
      "allocMb": [
        152.81
      ],
      "errorMs": [
        155.76
      ],
      "stdDevMs": [
        92.688
      ]
    },
    {
      "key": "OpenAndReadAll",
      "label": "Read \u00B7 open \u002B read all cells",
      "libraries": [
        "IronXL"
      ],
      "snapshotOf": [
        "2026.8.1, Job-HTCPYF, captured 2026-08-03"
      ],
      "timeMs": [
        9318.11
      ],
      "allocMb": [
        6333.04
      ],
      "errorMs": [
        477.33
      ],
      "stdDevMs": [
        315.727
      ]
    },
    {
      "key": "CreateAndSave",
      "label": "Write \u00B7 create \u002B save",
      "libraries": [
        "IronXL"
      ],
      "snapshotOf": [
        "2026.8.1, Job-HTCPYF, captured 2026-08-03"
      ],
      "timeMs": [
        943.14
      ],
      "allocMb": [
        797.63
      ],
      "errorMs": [
        18.52
      ],
      "stdDevMs": [
        11.02
      ]
    },
    {
      "key": "CreateStockReport",
      "label": "Report \u00B7 data \u002B conditional formatting \u002B chart",
      "libraries": [
        "IronXL"
      ],
      "snapshotOf": [
        "2026.8.1, Job-HTCPYF, captured 2026-08-03"
      ],
      "timeMs": [
        387.85
      ],
      "allocMb": [
        237.38
      ],
      "errorMs": [
        36.55
      ],
      "stdDevMs": [
        24.173
      ]
    },
    {
      "key": "EditAndRecalculate",
      "label": "Edit \u00B7 delete rows \u002B set column \u002B recalculate",
      "libraries": [
        "IronXL"
      ],
      "snapshotOf": [
        "2026.8.1, Job-HTCPYF, captured 2026-08-03"
      ],
      "timeMs": [
        1577.27
      ],
      "allocMb": [
        753.86
      ],
      "errorMs": [
        62.96
      ],
      "stdDevMs": [
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
        13.68,
        18.01,
        27.23,
        34.65,
        39.53,
        113.28
      ],
      "allocMb": [
        4.73,
        11.41,
        14.01,
        13.05,
        30.72,
        104.88
      ],
      "errorMs": [
        0.19,
        0.36,
        0.52,
        0.66,
        0.79,
        41.97
      ],
      "stdDevMs": [
        0.157,
        0.743,
        1.285,
        1.209,
        1.9,
        27.764
      ]
    }
  ]
};
