window.XLBENCH_DATA = {
  "updated": "2026-09-17 10:25:31Z",
  "versions": {
    "ClosedXML": "0.105.1",
    "EPPlus": "8.7.0",
    "OpenXML SDK": "3.5.1",
    "NPOI": "2.8.0",
    "MiniExcel": "1.46.0",
    "XLibur": "0.610.0",
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
        8.78,
        15.67,
        23.65,
        35.85,
        84.96,
        208.75
      ],
      "allocMb": [
        1.9,
        1.73,
        13.88,
        13.2,
        185.12,
        152.81
      ],
      "errorMs": [
        0.11,
        0.29,
        0.66,
        0.73,
        2.41,
        155.76
      ],
      "stdDevMs": [
        0.049,
        0.212,
        0.988,
        0.977,
        3.374,
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
        743.11,
        912.29,
        1169.26,
        1183.62,
        2547.31,
        5790.22,
        6757.99,
        9318.11
      ],
      "allocMb": [
        806.7,
        189.53,
        627.82,
        925.22,
        1077.67,
        4153.39,
        1083.78,
        6333.04
      ],
      "errorMs": [
        16.82,
        16.22,
        19.81,
        33.06,
        50.06,
        175.03,
        131.46,
        477.33
      ],
      "stdDevMs": [
        24.117,
        7.203,
        10.363,
        49.483,
        57.648,
        261.98,
        161.439,
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
        62.25,
        155.69,
        225.47,
        395.77,
        440.71,
        687.43,
        943.14,
        1700.53
      ],
      "allocMb": [
        84.59,
        134.19,
        60.5,
        181.09,
        322.83,
        247.27,
        797.63,
        2085.81
      ],
      "errorMs": [
        1.23,
        2.9,
        4.27,
        7.1,
        8.62,
        13.38,
        18.52,
        33.48
      ],
      "stdDevMs": [
        1.687,
        1.92,
        4.192,
        3.152,
        8.463,
        15.406,
        11.02,
        41.115
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
        8.21,
        9.61,
        15.12,
        16.98,
        31.93,
        178.45,
        387.85
      ],
      "allocMb": [
        4.92,
        3.47,
        13.92,
        8.02,
        16.23,
        429.67,
        237.38
      ],
      "errorMs": [
        0.16,
        0.28,
        0.27,
        0.33,
        0.61,
        37.25,
        36.55
      ],
      "stdDevMs": [
        0.095,
        0.417,
        0.07,
        0.371,
        0.273,
        55.754,
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
        11.13,
        26.07,
        87.26,
        346.03,
        398.96,
        1577.27,
        30771.49
      ],
      "allocMb": [
        3.55,
        9.8,
        142.49,
        337.71,
        413.38,
        753.86,
        59808.17
      ],
      "errorMs": [
        0.13,
        0.48,
        1.72,
        6.66,
        7.22,
        62.96,
        594.14
      ],
      "stdDevMs": [
        0.02,
        0.317,
        2.296,
        6.84,
        2.576,
        41.643,
        353.561
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
        13.2,
        18.18,
        29.05,
        40.02,
        45.7,
        113.28,
        222.54
      ],
      "allocMb": [
        3.87,
        11.41,
        13.64,
        13.09,
        30.77,
        104.88,
        345.16
      ],
      "errorMs": [
        0.25,
        0.64,
        1.19,
        1.33,
        3.32,
        41.97,
        8.74
      ],
      "stdDevMs": [
        0.301,
        0.87,
        1.739,
        1.953,
        4.873,
        27.764,
        12.53
      ]
    }
  ]
};
