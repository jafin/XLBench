window.XLBENCH_DATA = {
  "updated": "2026-10-04 17:15:28Z",
  "versions": {
    "ClosedXML": "0.105.1",
    "EPPlus": "8.7.1",
    "OpenXML SDK": "3.5.1",
    "NPOI": "2.8.1",
    "MiniExcel": "1.46.0",
    "Sylvan": "0.5.8",
    "XLibur": "0.630.1-rc.18",
    "OfficeIMO": "3.4.4",
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
        "XLibur",
        "NPOI",
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
        0.29,
        4.64,
        8.99,
        23.6,
        23.61,
        37.26,
        82.89,
        208.75
      ],
      "allocMb": [
        0.34,
        1.37,
        1.9,
        6.58,
        13.87,
        13.2,
        185.57,
        152.81
      ],
      "errorMs": [
        0.01,
        0.09,
        0.13,
        0.24,
        0.34,
        0.68,
        1.65,
        155.76
      ],
      "stdDevMs": [
        0.013,
        0.166,
        0.125,
        0.184,
        0.303,
        1.754,
        2.259,
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
        299.55,
        724.37,
        772.01,
        1105.22,
        1163.49,
        2444.67,
        3062.71,
        5361.57,
        6590.82,
        9318.11
      ],
      "allocMb": [
        25.28,
        806.71,
        136.76,
        627.82,
        925.23,
        1075.67,
        824.06,
        4169.57,
        1082.51,
        6333.04
      ],
      "errorMs": [
        3.03,
        7.21,
        9.79,
        13.33,
        20.36,
        46.66,
        43.61,
        45.66,
        43.12,
        477.33
      ],
      "stdDevMs": [
        2.835,
        6.74,
        9.154,
        12.466,
        18.048,
        49.921,
        38.657,
        40.48,
        38.227,
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
        38.37,
        58.78,
        150.72,
        180.73,
        379.28,
        432.71,
        641.24,
        938.79,
        943.14,
        1686.14
      ],
      "allocMb": [
        13.27,
        84.59,
        134.19,
        60.19,
        181.09,
        322.89,
        246.26,
        361.06,
        797.63,
        2097
      ],
      "errorMs": [
        0.77,
        1.16,
        1.6,
        3.45,
        7.53,
        6.53,
        12.56,
        18.17,
        18.52,
        23.1
      ],
      "stdDevMs": [
        1.075,
        1.55,
        1.416,
        7.206,
        10.559,
        6.106,
        13.434,
        17,
        11.02,
        21.608
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
        7.48,
        7.85,
        14.77,
        16.05,
        31.06,
        138.28,
        211.22,
        387.85
      ],
      "allocMb": [
        3.35,
        4.92,
        13.92,
        8.02,
        16.43,
        432.54,
        904.7,
        237.38
      ],
      "errorMs": [
        0.12,
        0.11,
        0.27,
        0.26,
        0.58,
        3.11,
        3.76,
        36.55
      ],
      "stdDevMs": [
        0.111,
        0.101,
        0.262,
        0.23,
        0.541,
        9.127,
        4.612,
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
        "NPOI",
        "ClosedXML",
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
        8.13,
        25.55,
        108.89,
        426.53,
        431.64,
        1577.27,
        3044.6,
        6998.35
      ],
      "allocMb": [
        2.68,
        9.83,
        142.49,
        413.44,
        337.87,
        753.86,
        491.27,
        16837.76
      ],
      "errorMs": [
        0.15,
        0.5,
        8.77,
        8.46,
        28.82,
        62.96,
        54.74,
        222.53
      ],
      "stdDevMs": [
        0.184,
        0.496,
        24.142,
        17.666,
        79.378,
        41.643,
        85.225,
        620.324
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
        9.35,
        17.39,
        28.34,
        39.03,
        42.31,
        113.28,
        151.75,
        218.87
      ],
      "allocMb": [
        2.72,
        11.41,
        14.02,
        13.11,
        30.77,
        104.88,
        66.87,
        280.56
      ],
      "errorMs": [
        0.13,
        0.26,
        0.53,
        0.72,
        0.84,
        41.97,
        3.01,
        4.37
      ],
      "stdDevMs": [
        0.122,
        0.229,
        1.186,
        1.624,
        1.924,
        27.764,
        6.544,
        10.964
      ]
    }
  ]
};
