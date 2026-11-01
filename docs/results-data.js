window.XLBENCH_DATA = {
  "updated": "2026-11-01 12:01:43Z",
  "versions": {
    "ClosedXML": "0.105.1",
    "EPPlus": "8.7.2",
    "OpenXML SDK": "3.5.1",
    "NPOI": "2.8.1",
    "MiniExcel": "1.46.0",
    "Sylvan": "0.5.8",
    "XLibur": "0.730.0",
    "OfficeIMO": "3.5.3",
    "Telerik": "2026.3.1027.100",
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
        0.31,
        4.92,
        9.08,
        22.83,
        25.09,
        35.02,
        81.99,
        208.75
      ],
      "allocMb": [
        0.34,
        1.4,
        1.9,
        13.87,
        7.07,
        13.2,
        182.77,
        152.81
      ],
      "errorMs": [
        0.01,
        0.1,
        0.13,
        0.45,
        0.34,
        0.7,
        1.63,
        155.76
      ],
      "stdDevMs": [
        0.022,
        0.131,
        0.118,
        0.898,
        0.281,
        1.261,
        2.762,
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
        277,
        686.82,
        734.61,
        1129.98,
        1349.65,
        2586.04,
        3187.03,
        5177.67,
        6568.24,
        9318.11
      ],
      "allocMb": [
        25.29,
        136.77,
        806.71,
        628.84,
        925.22,
        1075.8,
        823.12,
        3924.1,
        1083.14,
        6333.04
      ],
      "errorMs": [
        4.89,
        13.6,
        12.33,
        6.19,
        65.81,
        51.19,
        52.62,
        100.07,
        92.76,
        477.33
      ],
      "stdDevMs": [
        4.573,
        13.358,
        10.935,
        5.171,
        189.886,
        62.868,
        46.642,
        136.983,
        86.765,
        315.727
      ]
    },
    {
      "key": "OpenAndReadAllSheets",
      "label": "Read \u00B7 20 sheets, shared strings \u002B styles",
      "libraries": [
        "Sylvan",
        "NPOI",
        "OpenXML SDK",
        "EPPlus",
        "XLibur",
        "OfficeIMO",
        "MiniExcel",
        "Telerik",
        "ClosedXML"
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
        null
      ],
      "timeMs": [
        82.67,
        411.96,
        448.24,
        477.26,
        906.04,
        1213.49,
        1331.07,
        2026.96,
        2282.94
      ],
      "allocMb": [
        21.61,
        211.99,
        163.32,
        360.63,
        830.44,
        509.91,
        1087.23,
        2367.43,
        1024.14
      ],
      "errorMs": [
        1.09,
        8.08,
        6.79,
        9.4,
        17.19,
        22.39,
        16.87,
        30.74,
        44.26
      ],
      "stdDevMs": [
        1.024,
        20.262,
        6.355,
        15.954,
        16.08,
        23.954,
        14.09,
        28.757,
        47.361
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
        37.54,
        62.37,
        152.08,
        191.24,
        400.93,
        434.51,
        672.27,
        943.14,
        1095.51,
        1642.42
      ],
      "allocMb": [
        13.27,
        84.59,
        134.19,
        60.19,
        181.1,
        322.89,
        246.52,
        797.63,
        406.96,
        1999.22
      ],
      "errorMs": [
        0.58,
        1.23,
        1.21,
        3.49,
        7.24,
        8.18,
        13.17,
        18.52,
        20.6,
        29.14
      ],
      "stdDevMs": [
        0.511,
        1.508,
        1.077,
        4.42,
        16.485,
        7.253,
        20.108,
        11.02,
        18.264,
        29.925
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
        7.68,
        8.33,
        15.1,
        16.63,
        31.27,
        131.29,
        226.45,
        387.85
      ],
      "allocMb": [
        3.35,
        4.92,
        13.99,
        8.01,
        16.46,
        426.53,
        905.97,
        237.38
      ],
      "errorMs": [
        0.15,
        0.14,
        0.28,
        0.28,
        0.4,
        3.76,
        4.45,
        36.55
      ],
      "stdDevMs": [
        0.293,
        0.126,
        0.526,
        0.263,
        0.37,
        10.431,
        7.427,
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
        7.55,
        26.16,
        89.05,
        357.59,
        395.19,
        1577.27,
        2835.29,
        5597.02
      ],
      "allocMb": [
        2.69,
        9.83,
        142.49,
        337.81,
        413.44,
        753.86,
        494.22,
        16600.17
      ],
      "errorMs": [
        0.15,
        0.35,
        1.71,
        7.06,
        7.84,
        62.96,
        53.87,
        108.23
      ],
      "stdDevMs": [
        0.2,
        0.323,
        1.601,
        8.403,
        8.711,
        41.643,
        59.875,
        101.239
      ]
    },
    {
      "key": "InsertColumnsAndRecalculate",
      "label": "Edit \u00B7 insert 2 columns \u002B recalculate",
      "libraries": [
        "XLibur",
        "EPPlus",
        "ClosedXML",
        "NPOI",
        "OpenXML SDK",
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
        8.54,
        16.78,
        26.58,
        30.92,
        38.2,
        113.28,
        160.14,
        201.91
      ],
      "allocMb": [
        2.72,
        11.41,
        13.67,
        30.77,
        13.11,
        104.88,
        70.3,
        272.63
      ],
      "errorMs": [
        0.17,
        0.32,
        0.52,
        0.82,
        0.76,
        41.97,
        3.58,
        6.79
      ],
      "stdDevMs": [
        0.267,
        0.344,
        0.7,
        2.373,
        1.883,
        27.764,
        10.447,
        19.594
      ]
    },
    {
      "key": "ChangePricesAndRecalculate",
      "label": "Calc \u00B7 change prices \u002B recalculate lookups and SUMIFS",
      "libraries": [
        "XLibur",
        "ClosedXML",
        "EPPlus",
        "NPOI"
      ],
      "snapshotOf": [
        null,
        null,
        null,
        null
      ],
      "timeMs": [
        56.54,
        139.14,
        143.93,
        221.41
      ],
      "allocMb": [
        11.97,
        48.8,
        129.21,
        156.4
      ],
      "errorMs": [
        0.9,
        2.56,
        2.76,
        4.38
      ],
      "stdDevMs": [
        2.296,
        5.044,
        5.186,
        7.315
      ]
    },
    {
      "key": "FillTemplateAndSave",
      "label": "Template \u00B7 fill Excel-authored template \u002B save",
      "libraries": [
        "XLibur",
        "OpenXML SDK",
        "ClosedXML",
        "NPOI",
        "EPPlus",
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
        null
      ],
      "timeMs": [
        18.54,
        18.6,
        24.73,
        26.45,
        35.91,
        94.96,
        414.85
      ],
      "allocMb": [
        7.12,
        13.55,
        17.65,
        21.11,
        34.79,
        32.28,
        853.8
      ],
      "errorMs": [
        0.37,
        0.37,
        0.49,
        0.61,
        0.91,
        1.88,
        8.23
      ],
      "stdDevMs": [
        0.698,
        0.803,
        1.023,
        1.717,
        2.576,
        4.399,
        15.657
      ]
    }
  ]
};
