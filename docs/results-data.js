window.XLBENCH_DATA = {
  "updated": "2026-11-02 10:10:12Z",
  "versions": {
    "ClosedXML": "0.105.1",
    "EPPlus": "8.7.2",
    "OpenXML SDK": "3.5.1",
    "NPOI": "2.8.1",
    "MiniExcel": "1.46.0",
    "Sylvan": "0.5.8",
    "XLibur": "0.800.0",
    "OfficeIMO": "3.5.3",
    "Telerik": "2026.3.1027.100",
    "IronXL": "2026.8.1"
  },
  "commercial": [
    "Telerik",
    "IronXL"
  ],
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
        4.7,
        8.5,
        23.44,
        24.72,
        34.37,
        77.61,
        208.75
      ],
      "allocMb": [
        0.34,
        1.38,
        1.89,
        13.81,
        7.01,
        13.18,
        182.64,
        152.81
      ],
      "errorMs": [
        0.01,
        0.09,
        0.11,
        0.46,
        0.43,
        0.67,
        1.54,
        155.76
      ],
      "stdDevMs": [
        0.012,
        0.156,
        0.095,
        0.62,
        0.334,
        0.661,
        3.313,
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
        261.14,
        646.62,
        696.43,
        1139.32,
        1175.12,
        2418.13,
        3039.47,
        4854.03,
        6183.97,
        9318.11
      ],
      "allocMb": [
        25.28,
        136.57,
        806.67,
        925.22,
        627.82,
        1075.67,
        822.85,
        3886.46,
        1073.5,
        6333.04
      ],
      "errorMs": [
        5.2,
        8.01,
        13.29,
        13.67,
        22.88,
        47.9,
        38.05,
        96.92,
        40.75,
        477.33
      ],
      "stdDevMs": [
        5.774,
        7.099,
        16.323,
        11.418,
        20.278,
        65.571,
        35.596,
        122.578,
        36.12,
        315.727
      ]
    },
    {
      "key": "OpenAndReadAllSheets",
      "label": "Read \u00B7 20 sheets, shared strings \u002B styles",
      "libraries": [
        "Sylvan",
        "NPOI",
        "XLibur",
        "OpenXML SDK",
        "EPPlus",
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
        81.76,
        403.36,
        419.44,
        434.28,
        456.39,
        1166.46,
        1333.84,
        2069.36,
        2284.19
      ],
      "allocMb": [
        21.6,
        211.8,
        177.16,
        162.81,
        360.63,
        509.39,
        1081.32,
        2374.31,
        1023.57
      ],
      "errorMs": [
        0.92,
        8.03,
        8.36,
        7.24,
        9.11,
        19.6,
        16.94,
        40.37,
        44.9
      ],
      "stdDevMs": [
        0.819,
        19.079,
        17.27,
        6.047,
        22.86,
        18.334,
        14.146,
        51.061,
        53.447
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
        37.48,
        61.18,
        155.46,
        175.42,
        392.8,
        424.19,
        665.24,
        943.14,
        1034.28,
        1679.34
      ],
      "allocMb": [
        13.27,
        84.59,
        134.19,
        60.19,
        181.1,
        322.83,
        246.27,
        797.63,
        406.69,
        1983.49
      ],
      "errorMs": [
        0.28,
        1.19,
        2.62,
        2.79,
        7.83,
        8.2,
        12.51,
        18.52,
        12.96,
        33.34
      ],
      "stdDevMs": [
        0.218,
        1.814,
        2.455,
        2.606,
        17.02,
        7.268,
        21.902,
        11.02,
        12.121,
        46.741
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
        7.32,
        8.26,
        14.2,
        15.91,
        30.18,
        130.91,
        208.36,
        387.85
      ],
      "allocMb": [
        3.28,
        4.92,
        13.92,
        8.01,
        16.43,
        427.44,
        905.82,
        237.38
      ],
      "errorMs": [
        0.13,
        0.15,
        0.27,
        0.23,
        0.47,
        2.98,
        4.15,
        36.55
      ],
      "stdDevMs": [
        0.118,
        0.15,
        0.326,
        0.205,
        0.436,
        8.443,
        7.899,
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
        7.41,
        24.45,
        85.15,
        333.92,
        389.85,
        1577.27,
        2734.66,
        5362.54
      ],
      "allocMb": [
        2.68,
        9.8,
        142.49,
        337.66,
        413.38,
        753.86,
        494.08,
        16624.97
      ],
      "errorMs": [
        0.12,
        0.34,
        1.69,
        5.66,
        6.58,
        62.96,
        25.81,
        87.07
      ],
      "stdDevMs": [
        0.114,
        0.303,
        2.2,
        5.02,
        5.833,
        41.643,
        22.879,
        81.449
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
        8.52,
        15.99,
        26.46,
        33.39,
        36.44,
        113.28,
        149.61,
        173.27
      ],
      "allocMb": [
        2.69,
        11.41,
        14.06,
        13.05,
        30.72,
        104.88,
        70.13,
        272.32
      ],
      "errorMs": [
        0.17,
        0.32,
        0.49,
        0.66,
        0.72,
        41.97,
        1.84,
        3.43
      ],
      "stdDevMs": [
        0.384,
        0.295,
        0.64,
        0.652,
        2.005,
        27.764,
        3.407,
        7.539
      ]
    },
    {
      "key": "ChangePricesAndRecalculate",
      "label": "Calc \u00B7 change prices \u002B recalculate lookups and SUMIFS",
      "libraries": [
        "XLibur",
        "EPPlus",
        "ClosedXML",
        "NPOI"
      ],
      "snapshotOf": [
        null,
        null,
        null,
        null
      ],
      "timeMs": [
        55.44,
        133.12,
        137.38,
        194.35
      ],
      "allocMb": [
        10.76,
        129.21,
        48.78,
        156.21
      ],
      "errorMs": [
        0.95,
        2.5,
        1.73,
        3.49
      ],
      "stdDevMs": [
        2.438,
        2.783,
        1.441,
        5.92
      ]
    },
    {
      "key": "FillTemplateAndSave",
      "label": "Template \u00B7 fill Excel-authored template \u002B save",
      "libraries": [
        "XLibur",
        "OpenXML SDK",
        "NPOI",
        "ClosedXML",
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
        18.8,
        18.84,
        23.81,
        24.34,
        33.19,
        82.62,
        390.08
      ],
      "allocMb": [
        7.21,
        13.48,
        21.08,
        17.61,
        34.71,
        31.34,
        853.98
      ],
      "errorMs": [
        0.46,
        0.37,
        0.48,
        0.47,
        0.61,
        1.13,
        7.7
      ],
      "stdDevMs": [
        1.295,
        0.727,
        0.906,
        0.957,
        0.575,
        1.626,
        7.563
      ]
    }
  ]
};
