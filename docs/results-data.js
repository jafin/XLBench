window.XLBENCH_DATA = {
  "updated": "2026-07-30 17:02:44Z",
  "versions": {
    "ClosedXML": "0.105.1",
    "EPPlus": "8.6.3",
    "OpenXML SDK": "3.5.1",
    "NPOI": "2.8.0",
    "MiniExcel": "1.45.0",
    "XLibur": "0.106.1-beta.80",
    "IronXL": "2026.7.2"
  },
  "snapshots": {},
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
        null
      ],
      "timeMs": [
        250.08,
        1403.59,
        1746.25,
        3521.71,
        4373.98
      ],
      "allocMb": [
        211.37,
        158.39,
        1038.89,
        1306.4,
        7238.02
      ],
      "errorMs": [
        4.9,
        23.82,
        15.24,
        30.68,
        42.64
      ],
      "stdDevMs": [
        7.628,
        22.28,
        13.508,
        27.201,
        39.883
      ]
    },
    {
      "key": "OpenAndReadAll",
      "label": "Read \u00B7 open \u002B read all cells",
      "libraries": [
        "EPPlus",
        "OpenXML SDK",
        "XLibur",
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
        null,
        null
      ],
      "timeMs": [
        2308.52,
        2399.57,
        2414.63,
        4024.21,
        5153.54,
        16204.92,
        20254.6
      ],
      "allocMb": [
        1853.58,
        1255.32,
        659.64,
        1350.26,
        2157.21,
        12477.83,
        2176.62
      ],
      "errorMs": [
        24.06,
        41.41,
        44.77,
        57.93,
        93.81,
        268.28,
        106.66
      ],
      "stdDevMs": [
        20.094,
        63.241,
        41.875,
        51.353,
        83.163,
        250.949,
        94.55
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
        null
      ],
      "timeMs": [
        62.68,
        166.5,
        243.72,
        412.52,
        471.63,
        680.99,
        1017.1
      ],
      "allocMb": [
        84.59,
        134.19,
        67.46,
        181.1,
        322.9,
        247.52,
        796.44
      ],
      "errorMs": [
        1.25,
        3.05,
        4.71,
        7.93,
        9.34,
        12.61,
        19.65
      ],
      "stdDevMs": [
        1.336,
        2.55,
        6.445,
        7.789,
        17.072,
        12.383,
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
        null
      ],
      "timeMs": [
        9.07,
        9.97,
        15.68,
        18.43,
        31.61,
        371.31
      ],
      "allocMb": [
        4.92,
        3.82,
        14,
        8.02,
        16.26,
        241.78
      ],
      "errorMs": [
        0.18,
        0.19,
        0.27,
        0.55,
        0.51,
        6.83
      ],
      "stdDevMs": [
        0.3,
        0.285,
        0.388,
        1.558,
        0.542,
        5.7
      ]
    }
  ]
};
