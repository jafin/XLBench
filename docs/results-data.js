window.XLBENCH_DATA = {
  "updated": "2026-09-26 20:18:31Z",
  "versions": {
    "ClosedXML": "0.105.1",
    "EPPlus": "8.7.1",
    "OpenXML SDK": "3.5.1",
    "NPOI": "2.8.1",
    "MiniExcel": "1.46.0",
    "Sylvan": "0.5.8",
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
        8.94,
        15.15,
        22.87,
        24.65,
        34.78,
        85.08,
        208.75
      ],
      "allocMb": [
        1.89,
        1.73,
        13.81,
        6.58,
        13.18,
        184.77,
        152.81
      ],
      "errorMs": [
        0.18,
        0.3,
        0.45,
        0.3,
        0.51,
        1.7,
        155.76
      ],
      "stdDevMs": [
        0.256,
        0.365,
        0.917,
        0.238,
        0.452,
        1.507,
        92.688
      ]
    },
    {
      "key": "OpenAndReadAll",
      "label": "Read \u00B7 open \u002B read all cells",
      "libraries": [
        "Sylvan",
        "MiniExcel",
        "XLibur",
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
        266.2,
        742.33,
        947.65,
        1146.98,
        1185.79,
        2554.55,
        3270.16,
        5643.7,
        6481.11,
        9318.11
      ],
      "allocMb": [
        25.28,
        806.68,
        201.35,
        925.22,
        627.82,
        1075.67,
        824.06,
        4136.75,
        1074.77,
        6333.04
      ],
      "errorMs": [
        5.27,
        14.83,
        17.83,
        19.58,
        23.29,
        49.3,
        63.76,
        105.25,
        77.01,
        477.33
      ],
      "stdDevMs": [
        9.638,
        23.942,
        28.281,
        26.808,
        32.65,
        69.114,
        73.426,
        93.299,
        68.266,
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
        38.83,
        65.4,
        162.61,
        226.87,
        396.41,
        444.55,
        683.27,
        943.14,
        952.65,
        1757.64
      ],
      "allocMb": [
        13.27,
        84.59,
        134.19,
        60.5,
        181.09,
        322.83,
        246.26,
        797.63,
        357.23,
        2096.99
      ],
      "errorMs": [
        0.76,
        1.29,
        3.25,
        4.49,
        7.73,
        8.59,
        13.65,
        18.52,
        17.1,
        33.79
      ],
      "stdDevMs": [
        0.959,
        1.845,
        4.104,
        7.509,
        12.265,
        8.818,
        25.306,
        11.02,
        15.999,
        36.153
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
        8.04,
        9.23,
        15.62,
        16.62,
        31.98,
        141.36,
        223.38,
        387.85
      ],
      "allocMb": [
        4.92,
        3.35,
        13.92,
        8.02,
        16.43,
        432.08,
        904.69,
        237.38
      ],
      "errorMs": [
        0.15,
        0.18,
        0.31,
        0.31,
        0.63,
        3.94,
        4.41,
        36.55
      ],
      "stdDevMs": [
        0.134,
        0.204,
        0.458,
        0.29,
        0.996,
        11.605,
        7.123,
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
        11.96,
        24.79,
        86.57,
        345.98,
        393.75,
        1577.27,
        2739.03,
        6204.83
      ],
      "allocMb": [
        3.63,
        9.8,
        142.49,
        337.66,
        413.38,
        753.86,
        491.24,
        16844.2
      ],
      "errorMs": [
        0.23,
        0.45,
        1.72,
        6.8,
        7.73,
        62.96,
        53.53,
        123.57
      ],
      "stdDevMs": [
        0.212,
        0.654,
        2.471,
        8.84,
        8.268,
        41.643,
        67.699,
        160.67
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
        13.87,
        16.61,
        26.38,
        34.69,
        38.8,
        113.28,
        140.16,
        195.03
      ],
      "allocMb": [
        3.99,
        11.41,
        14.03,
        13.05,
        30.72,
        104.88,
        66.74,
        280.14
      ],
      "errorMs": [
        0.26,
        0.32,
        0.52,
        0.69,
        0.76,
        41.97,
        2.87,
        3.88
      ],
      "stdDevMs": [
        0.506,
        0.819,
        0.85,
        0.763,
        1.485,
        27.764,
        8.148,
        10.15
      ]
    }
  ]
};
