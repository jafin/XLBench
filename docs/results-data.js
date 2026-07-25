window.XLBENCH_DATA = {
  "updated": "2026-07-26 00:02:48Z",
  "versions": {
    "ClosedXML": "0.105.0",
    "EPPlus": "8.6.3",
    "OpenXML SDK": "3.5.1",
    "NPOI": "2.8.0",
    "MiniExcel": "1.45.0",
    "XLibur": "0.106.0"
  },
  "scenarios": [
    {
      "key": "OpenWorkbook",
      "label": "Read \u00B7 open workbook",
      "libraries": [
        "NPOI",
        "XLibur",
        "EPPlus",
        "ClosedXML"
      ],
      "timeMs": [
        245.8,
        1581.27,
        1760.66,
        3425.69
      ],
      "allocMb": [
        211.34,
        158.36,
        1038.89,
        1304.39
      ],
      "errorMs": [
        4.77,
        31.47,
        34.64,
        51.72
      ],
      "stdDevMs": [
        9.299,
        82.898,
        76.758,
        48.381
      ]
    },
    {
      "key": "OpenAndReadAll",
      "label": "Read \u00B7 open \u002B read all cells",
      "libraries": [
        "XLibur",
        "EPPlus",
        "OpenXML SDK",
        "MiniExcel",
        "NPOI",
        "ClosedXML"
      ],
      "timeMs": [
        2318.64,
        2382.38,
        2451.08,
        4018.41,
        5242.62,
        19951.98
      ],
      "allocMb": [
        643.61,
        1853.58,
        1253.3,
        1350.31,
        2157.09,
        2161.88
      ],
      "errorMs": [
        40.42,
        44.56,
        46.56,
        55.8,
        103.77,
        318.74
      ],
      "stdDevMs": [
        104.337,
        78.045,
        47.811,
        52.191,
        197.425,
        298.147
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
        "NPOI"
      ],
      "timeMs": [
        61.78,
        156.88,
        269.41,
        383.85,
        444.65,
        690.42
      ],
      "allocMb": [
        84.59,
        134.19,
        131.12,
        181.09,
        322.83,
        247.27
      ],
      "errorMs": [
        1.22,
        1.78,
        5.19,
        7.5,
        8.75,
        13.07
      ],
      "stdDevMs": [
        1.197,
        1.666,
        4.597,
        10.99,
        8.594,
        19.569
      ]
    }
  ]
};
