window.XLBENCH_DATA = {
  "updated": "2026-09-26 16:58:16Z",
  "versions": {
    "ClosedXML": "0.105.1",
    "EPPlus": "8.7.1",
    "OpenXML SDK": "3.5.1",
    "NPOI": "2.8.1",
    "MiniExcel": "1.46.0",
    "XLibur": "0.620.0",
    "OfficeIMO": "3.4.3",
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
        "OfficeIMO",
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
        null,
        "2026.8.1, Job-HTCPYF, captured 2026-08-03"
      ],
      "timeMs": [
        8.64,
        15.79,
        22.4,
        24.49,
        34.27,
        88.7,
        208.75
      ],
      "allocMb": [
        1.89,
        1.73,
        13.81,
        6.58,
        13.18,
        185.2,
        152.81
      ],
      "errorMs": [
        0.11,
        0.31,
        0.44,
        0.33,
        0.67,
        1.25,
        155.76
      ],
      "stdDevMs": [
        0.099,
        0.276,
        0.535,
        0.277,
        0.719,
        0.973,
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
        "OfficeIMO",
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
        null,
        "2026.8.1, Job-HTCPYF, captured 2026-08-03"
      ],
      "timeMs": [
        712.75,
        928.2,
        1147.7,
        1179.46,
        2551.87,
        3190.42,
        5786.26,
        6280.72,
        9318.11
      ],
      "allocMb": [
        806.68,
        201.35,
        627.82,
        925.22,
        1075.67,
        824.06,
        4169.56,
        1074.78,
        6333.04
      ],
      "errorMs": [
        11.51,
        17.25,
        12.63,
        11.35,
        50.89,
        62.37,
        115.72,
        121.25,
        477.33
      ],
      "stdDevMs": [
        10.77,
        16.941,
        11.197,
        10.613,
        56.566,
        74.248,
        146.352,
        113.414,
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
        "OfficeIMO",
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
        null,
        "2026.8.1, Job-HTCPYF, captured 2026-08-03",
        null
      ],
      "timeMs": [
        61.51,
        165.52,
        228.37,
        389.85,
        435.49,
        670.49,
        928.97,
        943.14,
        1716.01
      ],
      "allocMb": [
        84.59,
        134.19,
        60.5,
        181.09,
        322.83,
        246.26,
        357.23,
        797.63,
        2097.01
      ],
      "errorMs": [
        1.17,
        3.26,
        4.51,
        7.47,
        5.45,
        13.04,
        18.51,
        18.52,
        28.62
      ],
      "stdDevMs": [
        1.397,
        3.201,
        7.413,
        9.178,
        5.095,
        12.198,
        19.011,
        11.02,
        25.371
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
        "OfficeIMO",
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
        8.03,
        9.03,
        15.34,
        16.77,
        31.37,
        149.2,
        230.1,
        387.85
      ],
      "allocMb": [
        4.92,
        3.35,
        13.92,
        8.02,
        16.43,
        432.13,
        904.68,
        237.38
      ],
      "errorMs": [
        0.08,
        0.17,
        0.31,
        0.33,
        0.6,
        2.96,
        4.57,
        36.55
      ],
      "stdDevMs": [
        0.067,
        0.155,
        0.74,
        0.352,
        0.592,
        6.235,
        4.055,
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
        "OfficeIMO",
        "Telerik"
      ],
      "snapshotOf": [
        null,
        null,
        null,
        null,
        null,
        "2026.8.1, Job-HTCPYF, captured 2026-08-03",
        null,
        null
      ],
      "timeMs": [
        11.89,
        25.34,
        84.19,
        340.08,
        389.9,
        1577.27,
        2741.01,
        6367.98
      ],
      "allocMb": [
        3.63,
        9.8,
        142.49,
        337.69,
        413.38,
        753.86,
        491.24,
        16819.16
      ],
      "errorMs": [
        0.18,
        0.48,
        1.63,
        6.53,
        7.7,
        62.96,
        31,
        127.15
      ],
      "stdDevMs": [
        0.149,
        0.593,
        1.521,
        8.255,
        8.557,
        41.643,
        27.482,
        229.282
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
        "OfficeIMO",
        "Telerik"
      ],
      "snapshotOf": [
        null,
        null,
        null,
        null,
        null,
        "2026.8.1, Job-HTCPYF, captured 2026-08-03",
        null,
        null
      ],
      "timeMs": [
        13.7,
        16.02,
        26.31,
        33.56,
        38.45,
        113.28,
        134.87,
        196.97
      ],
      "allocMb": [
        3.99,
        11.41,
        14.01,
        13.05,
        30.72,
        104.88,
        66.69,
        280.09
      ],
      "errorMs": [
        0.27,
        0.32,
        0.53,
        0.66,
        0.81,
        41.97,
        2.67,
        4.26
      ],
      "stdDevMs": [
        0.475,
        0.605,
        1.024,
        0.755,
        2.259,
        27.764,
        5.464,
        12.027
      ]
    }
  ]
};
