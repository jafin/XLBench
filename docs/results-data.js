window.XLBENCH_DATA = {
  "updated": "2026-09-27 16:23:01Z",
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
        8.56,
        15.2,
        22.63,
        23.25,
        34.8,
        83.07,
        208.75
      ],
      "allocMb": [
        0.34,
        1.9,
        1.73,
        13.88,
        6.64,
        13.2,
        185.6,
        152.81
      ],
      "errorMs": [
        0.01,
        0.08,
        0.26,
        0.37,
        0.2,
        0.69,
        1.65,
        155.76
      ],
      "stdDevMs": [
        0.013,
        0.077,
        0.241,
        0.324,
        0.168,
        0.763,
        2.203,
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
        255.41,
        686.73,
        847.11,
        1093.31,
        1138.73,
        2408.33,
        3072.61,
        5336.46,
        6156.22,
        9318.11
      ],
      "allocMb": [
        25.28,
        806.71,
        138.58,
        925.23,
        627.82,
        1075.8,
        824.33,
        4169.56,
        1083.78,
        6333.04
      ],
      "errorMs": [
        2.82,
        7.25,
        16.4,
        18.46,
        13.65,
        33.39,
        32.13,
        68.01,
        44.32,
        477.33
      ],
      "stdDevMs": [
        2.353,
        6.431,
        19.521,
        17.268,
        12.769,
        31.234,
        28.482,
        63.612,
        39.288,
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
        37.24,
        60.46,
        160.36,
        219.46,
        383.28,
        425.09,
        636.39,
        900.8,
        943.14,
        1669.57
      ],
      "allocMb": [
        13.27,
        84.59,
        134.19,
        60.5,
        181.09,
        322.83,
        246.27,
        357.24,
        797.63,
        2097.01
      ],
      "errorMs": [
        0.72,
        1.19,
        2.86,
        4.36,
        4.93,
        7.03,
        11.76,
        16.31,
        18.52,
        22.95
      ],
      "stdDevMs": [
        0.802,
        1.164,
        2.675,
        6.524,
        4.607,
        6.579,
        10.424,
        15.256,
        11.02,
        21.471
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
        8.78,
        14.32,
        16.1,
        30.71,
        133.16,
        212.3,
        387.85
      ],
      "allocMb": [
        4.92,
        3.35,
        13.92,
        8.01,
        16.43,
        432.29,
        904.69,
        237.38
      ],
      "errorMs": [
        0.1,
        0.17,
        0.22,
        0.27,
        0.42,
        3.3,
        3.31,
        36.55
      ],
      "stdDevMs": [
        0.095,
        0.155,
        0.204,
        0.249,
        0.372,
        9.673,
        2.766,
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
        10.94,
        24.6,
        88.04,
        341.24,
        390.29,
        1577.27,
        2648.36,
        6238.29
      ],
      "allocMb": [
        2.77,
        9.8,
        142.49,
        337.8,
        413.38,
        753.86,
        491.14,
        16819.31
      ],
      "errorMs": [
        0.06,
        0.45,
        1.71,
        6.69,
        7.3,
        62.96,
        27.93,
        145.6
      ],
      "stdDevMs": [
        0.049,
        0.422,
        2.169,
        8.214,
        6.825,
        41.643,
        26.125,
        410.675
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
        12.12,
        16.56,
        26.05,
        36.36,
        37.79,
        113.28,
        136.9,
        186.07
      ],
      "allocMb": [
        2.87,
        11.41,
        13.7,
        13.11,
        30.76,
        104.88,
        66.71,
        280.43
      ],
      "errorMs": [
        0.13,
        0.32,
        0.52,
        0.72,
        0.76,
        41.97,
        2.7,
        3.7
      ],
      "stdDevMs": [
        0.11,
        0.332,
        0.579,
        1.589,
        1.736,
        27.764,
        5.21,
        8.927
      ]
    }
  ]
};
