window.XLBENCH_DATA = {
  "updated": "2026-09-01 14:23:59Z",
  "versions": {
    "ClosedXML": "0.105.1",
    "EPPlus": "8.7.0",
    "OpenXML SDK": "3.5.1",
    "NPOI": "2.8.0",
    "MiniExcel": "1.46.0",
    "XLibur": "0.311.2-alpha.34",
    "Telerik": "2026.3.826.100",
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
        8.61,
        15.04,
        22.61,
        32.62,
        80.29,
        208.75
      ],
      "allocMb": [
        1.89,
        1.72,
        13.82,
        13.18,
        184.79,
        152.81
      ],
      "errorMs": [
        0.15,
        0.26,
        0.27,
        0.54,
        1.57,
        155.76
      ],
      "stdDevMs": [
        0.038,
        0.153,
        0.095,
        0.238,
        1.309,
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
        708.58,
        894.63,
        1090.51,
        1106.33,
        2453.57,
        5312.91,
        6214.18,
        9318.11
      ],
      "allocMb": [
        806.7,
        189.9,
        627.82,
        925.22,
        1077.67,
        4153.39,
        1074.77,
        6333.04
      ],
      "errorMs": [
        12.16,
        15.56,
        21.64,
        14.67,
        48.37,
        105.23,
        121.26,
        477.33
      ],
      "stdDevMs": [
        6.359,
        10.292,
        9.607,
        3.809,
        31.992,
        46.725,
        18.766,
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
        60.37,
        150.04,
        225.26,
        387.45,
        430.05,
        662.34,
        943.14,
        1643.25
      ],
      "allocMb": [
        84.59,
        134.19,
        60.5,
        181.09,
        322.83,
        247.27,
        797.63,
        2085.84
      ],
      "errorMs": [
        1.2,
        2.41,
        4.31,
        7.71,
        7.64,
        11.41,
        18.52,
        32.61
      ],
      "stdDevMs": [
        1.289,
        0.625,
        5.291,
        7.573,
        4.547,
        5.967,
        11.02,
        17.053
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
        7.94,
        9.01,
        14.98,
        16.46,
        32.25,
        158.14,
        387.85
      ],
      "allocMb": [
        4.92,
        3.48,
        13.93,
        8.02,
        16.23,
        428.74,
        237.38
      ],
      "errorMs": [
        0.13,
        0.14,
        0.26,
        0.25,
        0.81,
        17.76,
        36.55
      ],
      "stdDevMs": [
        0.069,
        0.176,
        0.066,
        0.18,
        1.181,
        25.466,
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
        11.61,
        23.62,
        82.59,
        334.03,
        383.11,
        1577.27,
        29175.82
      ],
      "allocMb": [
        4.12,
        9.8,
        142.49,
        340.69,
        413.38,
        753.86,
        59758.94
      ],
      "errorMs": [
        0.19,
        0.22,
        1.28,
        2.56,
        7.54,
        62.96,
        427.5
      ],
      "stdDevMs": [
        0.126,
        0.057,
        1.198,
        0.395,
        4.488,
        41.643,
        152.449
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
        13.68,
        15.41,
        25.62,
        31.11,
        37.58,
        113.28,
        210.73
      ],
      "allocMb": [
        4.74,
        11.41,
        13.99,
        13.05,
        30.72,
        104.88,
        345.41
      ],
      "errorMs": [
        0.19,
        0.29,
        0.59,
        0.51,
        0.83,
        41.97,
        3.75
      ],
      "stdDevMs": [
        0.049,
        0.152,
        0.841,
        0.182,
        1.192,
        27.764,
        4.747
      ]
    }
  ]
};
