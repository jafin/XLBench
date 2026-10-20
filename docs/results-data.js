window.XLBENCH_DATA = {
  "updated": "2026-10-20 21:13:56Z",
  "versions": {
    "ClosedXML": "0.105.1",
    "EPPlus": "8.7.2",
    "OpenXML SDK": "3.5.1",
    "NPOI": "2.8.1",
    "MiniExcel": "1.46.0",
    "Sylvan": "0.5.8",
    "XLibur": "0.720.0",
    "OfficeIMO": "3.5.1",
    "Telerik": "2026.3.1014.100",
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
        "OpenXML SDK",
        "XLibur",
        "NPOI",
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
        null,
        "2026.8.1, Job-HTCPYF, captured 2026-08-03"
      ],
      "timeMs": [
        0.29,
        4.63,
        8.73,
        22.8,
        23.86,
        34.17,
        82.56,
        208.75
      ],
      "allocMb": [
        0.34,
        1.38,
        1.89,
        13.81,
        6.58,
        13.2,
        182.46,
        152.81
      ],
      "errorMs": [
        0.01,
        0.09,
        0.11,
        0.45,
        0.24,
        0.5,
        1.64,
        155.76
      ],
      "stdDevMs": [
        0.011,
        0.165,
        0.099,
        0.775,
        0.198,
        0.467,
        2.996,
        92.688
      ]
    },
    {
      "key": "OpenAndReadAll",
      "label": "Read \u00B7 open \u002B read all cells",
      "libraries": [
        "Sylvan",
        "XLibur",
        "MiniExcel",
        "EPPlus",
        "OpenXML SDK",
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
        null,
        "2026.8.1, Job-HTCPYF, captured 2026-08-03"
      ],
      "timeMs": [
        256.92,
        673.3,
        737.96,
        1178.71,
        1248.4,
        2506.96,
        3179.78,
        4964.71,
        6201.25,
        9318.11
      ],
      "allocMb": [
        25.28,
        136.57,
        806.68,
        925.22,
        627.82,
        1075.8,
        823.07,
        3920.02,
        1074.77,
        6333.04
      ],
      "errorMs": [
        4.61,
        12.99,
        9.22,
        23.57,
        14.13,
        42.07,
        36.29,
        83.55,
        45.63,
        477.33
      ],
      "stdDevMs": [
        4.09,
        16.889,
        8.627,
        25.216,
        12.529,
        39.353,
        33.942,
        78.148,
        35.626,
        315.727
      ]
    },
    {
      "key": "CreateAndSave",
      "label": "Write \u00B7 create \u002B save",
      "libraries": [
        "Sylvan",
        "MiniExcel",
        "OpenXML SDK",
        "XLibur",
        "ClosedXML",
        "EPPlus",
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
        null,
        null,
        "2026.8.1, Job-HTCPYF, captured 2026-08-03",
        null,
        null
      ],
      "timeMs": [
        37.91,
        61.92,
        159.53,
        182.26,
        399.79,
        442.02,
        669.57,
        943.14,
        1075.46,
        1686.84
      ],
      "allocMb": [
        13.27,
        84.59,
        134.19,
        60.19,
        181.09,
        322.83,
        246.26,
        797.63,
        406.69,
        1998.82
      ],
      "errorMs": [
        0.75,
        1.2,
        2.75,
        3.27,
        7.86,
        8.4,
        11.85,
        18.52,
        18.57,
        22.7
      ],
      "stdDevMs": [
        0.661,
        1.282,
        2.3,
        5.184,
        9.945,
        8.25,
        11.089,
        11.02,
        17.366,
        21.235
      ]
    },
    {
      "key": "CreateStockReport",
      "label": "Report \u00B7 data \u002B conditional formatting \u002B chart",
      "libraries": [
        "XLibur",
        "OpenXML SDK",
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
        7.63,
        7.83,
        15.13,
        16.85,
        30.83,
        134.25,
        217.05,
        387.85
      ],
      "allocMb": [
        3.35,
        4.92,
        13.92,
        8.02,
        16.43,
        426.15,
        905.81,
        237.38
      ],
      "errorMs": [
        0.13,
        0.06,
        0.29,
        0.32,
        0.37,
        2.76,
        4.34,
        36.55
      ],
      "stdDevMs": [
        0.123,
        0.049,
        0.29,
        0.359,
        0.344,
        8.017,
        6.753,
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
        7.46,
        26.36,
        87.85,
        342.96,
        401.59,
        1577.27,
        2825.16,
        5552.37
      ],
      "allocMb": [
        2.68,
        9.8,
        142.49,
        337.66,
        413.38,
        753.86,
        493.91,
        16582.68
      ],
      "errorMs": [
        0.15,
        0.35,
        1.24,
        2.76,
        7.76,
        62.96,
        28.84,
        78.13
      ],
      "stdDevMs": [
        0.21,
        0.314,
        1.156,
        2.302,
        9.238,
        41.643,
        26.976,
        73.083
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
        8.43,
        16.96,
        26.07,
        34.28,
        38.5,
        113.28,
        151.43,
        182.91
      ],
      "allocMb": [
        2.73,
        11.41,
        14,
        13.05,
        30.72,
        104.88,
        70.04,
        272.08
      ],
      "errorMs": [
        0.14,
        0.34,
        0.51,
        0.57,
        0.71,
        41.97,
        2.44,
        3.64
      ],
      "stdDevMs": [
        0.203,
        0.61,
        0.897,
        0.444,
        1.249,
        27.764,
        2.034,
        9.134
      ]
    }
  ]
};
