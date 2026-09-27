window.XLBENCH_DATA = {
  "updated": "2026-09-27 15:31:48Z",
  "versions": {
    "ClosedXML": "0.105.1",
    "EPPlus": "8.7.1",
    "OpenXML SDK": "3.5.1",
    "NPOI": "2.8.1",
    "MiniExcel": "1.46.0",
    "Sylvan": "0.5.8",
    "XLibur": "0.630.0",
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
        "OpenXML SDK",
        "NPOI",
        "XLibur",
        "OfficeIMO",
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
        null,
        null,
        "2026.8.1, Job-HTCPYF, captured 2026-08-03"
      ],
      "timeMs": [
        0.28,
        9.44,
        14.96,
        23.33,
        24.3,
        36.78,
        80.58,
        208.75
      ],
      "allocMb": [
        0.34,
        1.9,
        1.74,
        6.64,
        13.87,
        13.2,
        185.7,
        152.81
      ],
      "errorMs": [
        0.01,
        0.19,
        0.29,
        0.26,
        0.48,
        0.58,
        1.61,
        155.76
      ],
      "stdDevMs": [
        0.012,
        0.393,
        0.339,
        0.218,
        1.257,
        0.517,
        2.452,
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
        null,
        "2026.8.1, Job-HTCPYF, captured 2026-08-03"
      ],
      "timeMs": [
        257.28,
        784.87,
        846.12,
        1213.9,
        1224.66,
        2368.08,
        3022.89,
        5240.76,
        7024.05,
        9318.11
      ],
      "allocMb": [
        25.29,
        806.71,
        138.59,
        628.84,
        925.23,
        1075.8,
        824.33,
        4165.24,
        1083.78,
        6333.04
      ],
      "errorMs": [
        3.94,
        15.58,
        11.46,
        16.37,
        18.22,
        23.67,
        24.86,
        36.41,
        138.22,
        477.33
      ],
      "stdDevMs": [
        3.681,
        21.834,
        10.719,
        15.316,
        16.147,
        23.245,
        23.256,
        34.055,
        259.614,
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
        null,
        "2026.8.1, Job-HTCPYF, captured 2026-08-03",
        null
      ],
      "timeMs": [
        37.53,
        59.25,
        160.11,
        218.11,
        375.71,
        423.49,
        635.56,
        889.07,
        943.14,
        1655.1
      ],
      "allocMb": [
        13.27,
        84.59,
        134.19,
        60.5,
        181.09,
        322.83,
        246.27,
        357.23,
        797.63,
        2096.99
      ],
      "errorMs": [
        0.16,
        1.06,
        3.16,
        4.27,
        7.43,
        6.48,
        9.86,
        15.57,
        18.52,
        25
      ],
      "stdDevMs": [
        0.143,
        0.987,
        4.115,
        7.366,
        6.949,
        5.745,
        8.742,
        15.294,
        11.02,
        23.388
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
        8.36,
        8.68,
        14.46,
        15.86,
        31.38,
        132.24,
        209.92,
        387.85
      ],
      "allocMb": [
        4.92,
        3.35,
        13.99,
        8.02,
        16.47,
        432.07,
        904.85,
        237.38
      ],
      "errorMs": [
        0.12,
        0.12,
        0.28,
        0.21,
        0.61,
        3.44,
        4,
        36.55
      ],
      "stdDevMs": [
        0.114,
        0.108,
        0.373,
        0.201,
        0.598,
        10.08,
        5.981,
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
        11.6,
        26.07,
        84.19,
        346.26,
        391.07,
        1577.27,
        2755.52,
        6399.46
      ],
      "allocMb": [
        2.77,
        9.8,
        142.49,
        337.7,
        413.38,
        753.86,
        491.24,
        16818.4
      ],
      "errorMs": [
        0.22,
        0.38,
        1.59,
        5.29,
        7.53,
        62.96,
        26.74,
        127.05
      ],
      "stdDevMs": [
        0.265,
        0.337,
        1.49,
        4.692,
        6.289,
        41.643,
        23.7,
        250.774
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
        13.31,
        19.09,
        30.3,
        39.39,
        42.86,
        113.28,
        155.9,
        212.1
      ],
      "allocMb": [
        2.87,
        11.41,
        14.03,
        13.11,
        30.77,
        104.88,
        66.88,
        280.49
      ],
      "errorMs": [
        0.26,
        0.75,
        1.09,
        1.02,
        1.42,
        41.97,
        6.23,
        4.4
      ],
      "stdDevMs": [
        0.407,
        2.09,
        3.158,
        2.918,
        4.141,
        27.764,
        18.364,
        12.4
      ]
    }
  ]
};
