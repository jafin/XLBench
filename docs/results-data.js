window.XLBENCH_DATA = {
  "updated": "2026-07-24 15:25:22Z",
  "versions": {
    "ClosedXML": "0.105.0",
    "EPPlus": "8.6.2",
    "OpenXML SDK": "3.5.1",
    "NPOI": "2.8.0",
    "MiniExcel": "1.45.0",
    "XLibur": "0.105.1-rc.151"
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
        246.04,
        1709.82,
        1724.88,
        3588.13
      ],
      "allocMb": [
        211.34,
        410.92,
        1038.89,
        1304.39
      ],
      "errorMs": [
        200.98,
        568.3,
        738.25,
        656.43
      ],
      "stdDevMs": [
        11.016,
        31.151,
        40.466,
        35.981
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
        "ClosedXML"
      ],
      "timeMs": [
        2214.6,
        2262.21,
        2661.47,
        3554.01,
        5510.05,
        19930.42
      ],
      "allocMb": [
        1853.58,
        1253.3,
        912.19,
        1350.31,
        2157.21,
        2177.89
      ],
      "errorMs": [
        295.26,
        109.61,
        1820.8,
        656.31,
        9138.64,
        460.75
      ],
      "stdDevMs": [
        16.184,
        6.008,
        99.804,
        35.975,
        500.919,
        25.255
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
        63.02,
        162.53,
        296.32,
        417.89,
        441.59,
        715.29
      ],
      "allocMb": [
        84.59,
        134.19,
        131.13,
        181.09,
        322.83,
        247.27
      ],
      "errorMs": [
        27.36,
        78.9,
        237.17,
        67.69,
        103.02,
        296.7
      ],
      "stdDevMs": [
        1.5,
        4.325,
        13,
        3.71,
        5.647,
        16.263
      ]
    }
  ]
};
