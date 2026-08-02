window.XLBENCH_DATA = {
  "updated": "2026-08-02 21:43:03Z",
  "versions": {
    "ClosedXML": "0.105.1",
    "EPPlus": "8.6.3",
    "OpenXML SDK": "3.5.1",
    "NPOI": "2.8.0",
    "MiniExcel": "1.45.0",
    "XLibur": "0.300.0",
    "IronXL": "2026.8.1"
  },
  "snapshots": {
    "IronXL": "2026.8.1, Job-HTCPYF, captured 2026-08-01"
  },
  "scenarios": [
    {
      "key": "OpenWorkbook",
      "label": "Read \u00B7 open workbook",
      "libraries": [
        "NPOI",
        "XLibur",
        "EPPlus",
        "ClosedXML",
        "IronXL"
      ],
      "snapshotOf": [
        null,
        null,
        null,
        null,
        "2026.8.1, Job-HTCPYF, captured 2026-08-01"
      ],
      "timeMs": [
        116.3,
        691.8,
        845.31,
        1745.81,
        2201.53
      ],
      "allocMb": [
        105.92,
        80.77,
        518.21,
        654.05,
        3717.76
      ],
      "errorMs": [
        8.54,
        41.13,
        32.09,
        110.67,
        37.57
      ],
      "stdDevMs": [
        5.649,
        27.205,
        19.096,
        73.199,
        16.682
      ]
    },
    {
      "key": "OpenAndReadAll",
      "label": "Read \u00B7 open \u002B read all cells",
      "libraries": [
        "MiniExcel",
        "XLibur",
        "EPPlus",
        "OpenXML SDK",
        "NPOI",
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
        "2026.8.1, Job-HTCPYF, captured 2026-08-01"
      ],
      "timeMs": [
        667.42,
        1139.16,
        1175.4,
        1276.74,
        2677.59,
        6179.59,
        7788.2
      ],
      "allocMb": [
        629.7,
        320.43,
        925.22,
        628.84,
        1077.8,
        1083.14,
        6333.03
      ],
      "errorMs": [
        11.84,
        34.09,
        48.1,
        60.47,
        194.71,
        139.39,
        256.73
      ],
      "stdDevMs": [
        5.255,
        22.549,
        31.814,
        39.998,
        128.789,
        82.951,
        169.812
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
        "IronXL"
      ],
      "snapshotOf": [
        null,
        null,
        null,
        null,
        null,
        null,
        "2026.8.1, Job-HTCPYF, captured 2026-08-01"
      ],
      "timeMs": [
        61.83,
        154.01,
        236.31,
        395.24,
        433.17,
        697.06,
        920.66
      ],
      "allocMb": [
        84.59,
        134.19,
        60.52,
        181.09,
        322.9,
        247.52,
        797.63
      ],
      "errorMs": [
        3.62,
        2.83,
        10.63,
        17.49,
        9.64,
        31.3,
        65.94
      ],
      "stdDevMs": [
        2.392,
        1.482,
        7.033,
        11.571,
        5.739,
        18.624,
        43.613
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
        "IronXL"
      ],
      "snapshotOf": [
        null,
        null,
        null,
        null,
        null,
        "2026.8.1, Job-HTCPYF, captured 2026-08-01"
      ],
      "timeMs": [
        8.43,
        9.36,
        16.77,
        17.15,
        33.11,
        374.58
      ],
      "allocMb": [
        4.92,
        3.49,
        14,
        8.02,
        16.3,
        237.38
      ],
      "errorMs": [
        0.33,
        0.41,
        2.47,
        0.34,
        2.82,
        38.14
      ],
      "stdDevMs": [
        0.22,
        0.273,
        1.469,
        0.052,
        1.475,
        19.949
      ]
    },
    {
      "key": "EditAndRecalculate",
      "label": "Edit \u00B7 delete rows \u002B set column \u002B recalculate",
      "libraries": [
        "OpenXML SDK",
        "XLibur",
        "EPPlus",
        "ClosedXML",
        "NPOI",
        "IronXL"
      ],
      "snapshotOf": [
        null,
        null,
        null,
        null,
        null,
        "2026.8.1, Job-HTCPYF, captured 2026-08-01"
      ],
      "timeMs": [
        24.93,
        25.77,
        92.97,
        358.36,
        397.43,
        1562.62
      ],
      "allocMb": [
        9.83,
        4.39,
        142.49,
        337.78,
        413.44,
        753.86
      ],
      "errorMs": [
        0.31,
        16.57,
        7.94,
        16.01,
        11.21,
        46.52
      ],
      "stdDevMs": [
        0.048,
        10.959,
        5.25,
        9.526,
        6.668,
        27.684
      ]
    }
  ]
};
