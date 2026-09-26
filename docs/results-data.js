window.XLBENCH_DATA = {
  "updated": "2026-09-26 21:47:12Z",
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
        "OpenXML SDK",
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
        null,
        "2026.8.1, Job-HTCPYF, captured 2026-08-03"
      ],
      "timeMs": [
        0.29,
        8.62,
        14.67,
        22.08,
        23.34,
        34.03,
        79.82,
        208.75
      ],
      "allocMb": [
        0.34,
        1.89,
        1.73,
        13.81,
        6.58,
        13.18,
        185.61,
        152.81
      ],
      "errorMs": [
        0.01,
        0.11,
        0.19,
        0.44,
        0.33,
        0.67,
        1.49,
        155.76
      ],
      "stdDevMs": [
        0.01,
        0.105,
        0.157,
        0.643,
        0.254,
        0.922,
        2.837,
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
        254.77,
        708.57,
        897.58,
        1115.59,
        1130.08,
        2392.47,
        3118.84,
        5300.47,
        6158.84,
        9318.11
      ],
      "allocMb": [
        25.28,
        806.67,
        201.35,
        925.22,
        627.82,
        1075.67,
        824.06,
        4164.98,
        1073.5,
        6333.04
      ],
      "errorMs": [
        2.16,
        8.99,
        15.87,
        11,
        14.5,
        44.6,
        60.57,
        39.89,
        32.86,
        477.33
      ],
      "stdDevMs": [
        1.8,
        8.408,
        14.848,
        9.75,
        12.853,
        37.247,
        53.698,
        37.31,
        30.738,
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
        37.96,
        62.8,
        154.51,
        225.34,
        390.52,
        427.28,
        656.32,
        913.38,
        943.14,
        1646.94
      ],
      "allocMb": [
        13.27,
        84.59,
        134.19,
        60.5,
        181.09,
        322.83,
        246.26,
        357.24,
        797.63,
        2089.35
      ],
      "errorMs": [
        0.68,
        1.21,
        2.62,
        4.38,
        7.57,
        8.44,
        12.88,
        8.39,
        18.52,
        20.51
      ],
      "stdDevMs": [
        0.639,
        1.616,
        2.45,
        6.947,
        10.368,
        9.379,
        16.294,
        6.553,
        11.02,
        18.181
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
        7.96,
        8.68,
        14.58,
        16.26,
        31.49,
        142.69,
        215,
        387.85
      ],
      "allocMb": [
        4.92,
        3.35,
        13.92,
        8.02,
        16.43,
        432.28,
        904.69,
        237.38
      ],
      "errorMs": [
        0.09,
        0.12,
        0.28,
        0.31,
        0.59,
        3.96,
        4.29,
        36.55
      ],
      "stdDevMs": [
        0.076,
        0.097,
        0.303,
        0.323,
        0.58,
        11.096,
        4.211,
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
        11.9,
        24.44,
        83.82,
        331.89,
        382.41,
        1577.27,
        2688.41,
        5815.54
      ],
      "allocMb": [
        3.63,
        9.8,
        142.49,
        337.93,
        413.38,
        753.86,
        491.25,
        16818.88
      ],
      "errorMs": [
        0.24,
        0.27,
        1.49,
        4.24,
        5.74,
        62.96,
        11.47,
        113.54
      ],
      "stdDevMs": [
        0.577,
        0.252,
        1.324,
        3.544,
        5.368,
        41.643,
        9.577,
        166.431
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
        13.41,
        15.41,
        25.69,
        31.69,
        35.67,
        113.28,
        134.17,
        184.1
      ],
      "allocMb": [
        3.99,
        11.41,
        14,
        13.05,
        30.72,
        104.88,
        66.84,
        280.26
      ],
      "errorMs": [
        0.25,
        0.25,
        0.51,
        0.44,
        0.71,
        41.97,
        2.61,
        3.67
      ],
      "stdDevMs": [
        0.477,
        0.222,
        1.215,
        0.414,
        1.663,
        27.764,
        5.278,
        9.127
      ]
    }
  ]
};
