window.XLBENCH_DATA = {
  "updated": "2026-08-01 17:41:12Z",
  "versions": {
    "ClosedXML": "0.105.1",
    "EPPlus": "8.6.3",
    "OpenXML SDK": "3.5.1",
    "NPOI": "2.8.0",
    "MiniExcel": "1.45.0",
    "XLibur": "0.200.0",
    "IronXL": "2026.7.2"
  },
  "snapshots": {
    "IronXL": "2026.7.2, DefaultJob, captured 2026-07-30"
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
        "2026.7.2, DefaultJob, captured 2026-07-30"
      ],
      "timeMs": [
        240.33,
        1355.93,
        1704.54,
        3333.54,
        4373.98
      ],
      "allocMb": [
        211.37,
        158.32,
        1038.89,
        1306.4,
        7238.02
      ],
      "errorMs": [
        4.49,
        11.6,
        17.47,
        26.89,
        42.64
      ],
      "stdDevMs": [
        3.978,
        9.055,
        16.344,
        23.838,
        39.883
      ]
    },
    {
      "key": "OpenAndReadAll",
      "label": "Read \u00B7 open \u002B read all cells",
      "libraries": [
        "EPPlus",
        "XLibur",
        "OpenXML SDK",
        "MiniExcel",
        "NPOI",
        "IronXL",
        "ClosedXML"
      ],
      "snapshotOf": [
        null,
        null,
        null,
        null,
        null,
        "2026.7.2, DefaultJob, captured 2026-07-30",
        null
      ],
      "timeMs": [
        2228.46,
        2294.23,
        2381.53,
        4020.24,
        5046.17,
        16204.92,
        19705.19
      ],
      "allocMb": [
        1853.58,
        644.47,
        1255.32,
        1350.26,
        2157.21,
        12477.83,
        2177.89
      ],
      "errorMs": [
        23.63,
        34.99,
        21.8,
        18.21,
        69.33,
        268.28,
        77.26
      ],
      "stdDevMs": [
        22.106,
        31.017,
        20.393,
        16.14,
        64.851,
        250.949,
        72.27
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
        "2026.7.2, DefaultJob, captured 2026-07-30"
      ],
      "timeMs": [
        59.24,
        156.67,
        233.25,
        387.34,
        436.74,
        665.24,
        1017.1
      ],
      "allocMb": [
        84.59,
        134.19,
        60.52,
        181.1,
        322.9,
        247.52,
        796.44
      ],
      "errorMs": [
        1.03,
        1.55,
        4.59,
        7.19,
        8.7,
        11.97,
        19.65
      ],
      "stdDevMs": [
        0.964,
        1.376,
        8.041,
        7.057,
        10.022,
        11.196,
        43.96
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
        "2026.7.2, DefaultJob, captured 2026-07-30"
      ],
      "timeMs": [
        7.99,
        9.35,
        15.01,
        16.63,
        31.37,
        371.31
      ],
      "allocMb": [
        4.92,
        3.49,
        14,
        8.02,
        16.26,
        241.78
      ],
      "errorMs": [
        0.08,
        0.18,
        0.25,
        0.17,
        0.58,
        6.83
      ],
      "stdDevMs": [
        0.068,
        0.17,
        0.233,
        0.148,
        0.538,
        5.7
      ]
    }
  ]
};
