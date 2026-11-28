window.XLBENCH_DATA = {
  "updated": "2026-11-28 19:09:43Z",
  "versions": {
    "ClosedXML": "0.105.1",
    "EPPlus": "8.7.3",
    "OpenXML SDK": "3.5.1",
    "NPOI": "2.8.1",
    "MiniExcel": "2.0.0",
    "Sylvan": "0.5.8",
    "XLibur": "0.900.0",
    "OfficeIMO": "3.6.2",
    "Telerik": "2026.4.1127.100",
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
        "OfficeIMO",
        "XLibur",
        "NPOI",
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
        0.29,
        0.35,
        4.5,
        8.53,
        22.09,
        33.94,
        80.27,
        208.75
      ],
      "allocMb": [
        0.34,
        0.47,
        1.39,
        1.89,
        13.81,
        13.18,
        182.57,
        152.81
      ],
      "errorMs": [
        0.01,
        0,
        0.09,
        0.13,
        0.43,
        0.58,
        1.59,
        155.76
      ],
      "stdDevMs": [
        0.012,
        0.005,
        0.158,
        0.113,
        0.607,
        0.511,
        2.859,
        92.688
      ]
    },
    {
      "key": "OpenAndReadAll",
      "label": "Read \u00B7 open \u002B read all cells",
      "libraries": [
        "OfficeIMO",
        "Sylvan",
        "MiniExcel",
        "XLibur",
        "EPPlus",
        "OpenXML SDK",
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
        null,
        null,
        "2026.8.1, Job-HTCPYF, captured 2026-08-03"
      ],
      "timeMs": [
        135.63,
        258.31,
        558.19,
        663.53,
        1110.9,
        1117.59,
        2477.11,
        4772.25,
        6141.05,
        9318.11
      ],
      "allocMb": [
        49.67,
        25.28,
        221.18,
        136.58,
        925.22,
        627.82,
        1075.67,
        3895.6,
        1074.77,
        6333.04
      ],
      "errorMs": [
        2.6,
        4.46,
        9.28,
        9.44,
        16.87,
        9.57,
        47.94,
        72.03,
        103.29,
        477.33
      ],
      "stdDevMs": [
        2.995,
        4.171,
        9.525,
        8.371,
        15.781,
        7.989,
        57.064,
        63.849,
        96.613,
        315.727
      ]
    },
    {
      "key": "OpenAndReadAllSheets",
      "label": "Read \u00B7 20 sheets, shared strings \u002B styles",
      "libraries": [
        "OfficeIMO",
        "Sylvan",
        "NPOI",
        "XLibur",
        "OpenXML SDK",
        "EPPlus",
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
        50.99,
        81.89,
        395.82,
        430.8,
        458.09,
        474.39,
        1200.06,
        2020.21,
        2204.88
      ],
      "allocMb": [
        12.03,
        21.6,
        211.8,
        178.25,
        162.81,
        360.64,
        499.72,
        2377.63,
        1023.57
      ],
      "errorMs": [
        0.94,
        1.58,
        7.83,
        8.53,
        9.11,
        9.48,
        12.24,
        33.2,
        40.71
      ],
      "stdDevMs": [
        1.187,
        2.002,
        13.722,
        20.772,
        8.947,
        25.464,
        11.447,
        36.904,
        36.085
      ]
    },
    {
      "key": "CreateAndSave",
      "label": "Write \u00B7 create \u002B save",
      "libraries": [
        "OfficeIMO",
        "Sylvan",
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
        null,
        null,
        "2026.8.1, Job-HTCPYF, captured 2026-08-03",
        null
      ],
      "timeMs": [
        19.84,
        37.45,
        57.87,
        156.74,
        182.01,
        375.52,
        435.84,
        642.23,
        943.14,
        1603.37
      ],
      "allocMb": [
        9.38,
        13.27,
        81.81,
        134.19,
        60.21,
        181.09,
        322.83,
        246.26,
        797.63,
        1999.13
      ],
      "errorMs": [
        0.3,
        0.48,
        1.14,
        2.28,
        3.51,
        7.44,
        8.67,
        12.62,
        18.52,
        16.58
      ],
      "stdDevMs": [
        0.277,
        0.426,
        1.808,
        2.021,
        4.684,
        6.96,
        12.431,
        11.801,
        11.02,
        13.849
      ]
    },
    {
      "key": "CreateStockReport",
      "label": "Report \u00B7 data \u002B conditional formatting \u002B chart",
      "libraries": [
        "OfficeIMO",
        "XLibur",
        "OpenXML SDK",
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
        null,
        "2026.8.1, Job-HTCPYF, captured 2026-08-03"
      ],
      "timeMs": [
        3.8,
        7.38,
        8.23,
        14.33,
        16.32,
        30.46,
        129.29,
        387.85
      ],
      "allocMb": [
        2.92,
        3.29,
        4.92,
        13.92,
        8.27,
        16.43,
        426.5,
        237.38
      ],
      "errorMs": [
        0.08,
        0.14,
        0.15,
        0.28,
        0.29,
        0.6,
        3.46,
        36.55
      ],
      "stdDevMs": [
        0.083,
        0.185,
        0.137,
        0.434,
        0.276,
        0.67,
        9.65,
        24.173
      ]
    },
    {
      "key": "EditAndRecalculate",
      "label": "Edit \u00B7 delete rows \u002B set column \u002B recalculate",
      "libraries": [
        "OfficeIMO",
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
        null,
        "2026.8.1, Job-HTCPYF, captured 2026-08-03",
        null
      ],
      "timeMs": [
        6.62,
        7.21,
        25.62,
        86.06,
        343.97,
        387.64,
        1577.27,
        5406.19
      ],
      "allocMb": [
        3.63,
        2.69,
        9.8,
        142.52,
        337.74,
        413.38,
        753.86,
        16589.64
      ],
      "errorMs": [
        0.13,
        0.14,
        0.28,
        1.7,
        6.75,
        7.1,
        62.96,
        47.33
      ],
      "stdDevMs": [
        0.241,
        0.171,
        0.264,
        2.445,
        8.78,
        10.634,
        41.643,
        44.269
      ]
    },
    {
      "key": "InsertColumnsAndRecalculate",
      "label": "Edit \u00B7 insert 2 columns \u002B recalculate",
      "libraries": [
        "OfficeIMO",
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
        null,
        "2026.8.1, Job-HTCPYF, captured 2026-08-03",
        null
      ],
      "timeMs": [
        7.76,
        7.96,
        15.56,
        26.17,
        31.39,
        36.35,
        113.28,
        176.89
      ],
      "allocMb": [
        3.48,
        2.7,
        11.45,
        14.04,
        13.05,
        30.72,
        104.88,
        272.39
      ],
      "errorMs": [
        0.15,
        0.12,
        0.29,
        0.52,
        0.61,
        0.73,
        41.97,
        3.5
      ],
      "stdDevMs": [
        0.146,
        0.094,
        0.255,
        0.66,
        0.537,
        1.987,
        27.764,
        6.221
      ]
    },
    {
      "key": "ChangePricesAndRecalculate",
      "label": "Calc \u00B7 change prices \u002B recalculate lookups and SUMIFS",
      "libraries": [
        "OfficeIMO",
        "XLibur",
        "ClosedXML",
        "EPPlus",
        "NPOI"
      ],
      "snapshotOf": [
        null,
        null,
        null,
        null,
        null
      ],
      "timeMs": [
        36.11,
        54.05,
        132.18,
        138.96,
        202
      ],
      "allocMb": [
        15.25,
        10.77,
        48.78,
        129.6,
        156.21
      ],
      "errorMs": [
        0.72,
        1.56,
        2.23,
        2.41,
        4.01
      ],
      "stdDevMs": [
        0.799,
        4.059,
        3.729,
        3.685,
        7.634
      ]
    },
    {
      "key": "FillTemplateAndSave",
      "label": "Template \u00B7 fill Excel-authored template \u002B save",
      "libraries": [
        "OfficeIMO",
        "XLibur",
        "OpenXML SDK",
        "ClosedXML",
        "NPOI",
        "EPPlus",
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
        15.46,
        18.76,
        18.98,
        23.59,
        24.21,
        33.09,
        404.7
      ],
      "allocMb": [
        6.49,
        7.3,
        13.48,
        17.61,
        21.08,
        34.71,
        852.92
      ],
      "errorMs": [
        0.3,
        0.46,
        0.37,
        0.43,
        0.48,
        0.62,
        8.07
      ],
      "stdDevMs": [
        0.614,
        1.306,
        0.701,
        0.548,
        1.35,
        0.855,
        12.558
      ]
    }
  ]
};
