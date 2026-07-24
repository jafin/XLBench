window.XLBENCH_DATA = {
  "updated": "2026-07-24 12:57:40Z",
  "versions": {
    "ClosedXML": "0.105.0",
    "EPPlus": "8.6.2",
    "OpenXML SDK": "3.5.1",
    "NPOI": "2.8.0",
    "MiniExcel": "1.45.0",
    "XLibur": "0.105.1-rc.137"
  },
  "scenarios": [
    {
      "key": "OpenWorkbook",
      "label": "Read \u00B7 open workbook",
      "libraries": [
        "NPOI",
        "EPPlus",
        "XLibur",
        "ClosedXML"
      ],
      "timeMs": [
        272.68,
        1694.42,
        2513.02,
        3460.56
      ],
      "allocMb": [
        211.34,
        1038.89,
        947.64,
        1304.39
      ],
      "errorMs": [
        177.75,
        584.85,
        501.61,
        1465.4
      ],
      "stdDevMs": [
        9.743,
        32.058,
        27.495,
        80.323
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
        2181.29,
        2378.87,
        3507.72,
        3567.95,
        5496.43,
        20459.94
      ],
      "allocMb": [
        1853.58,
        1253.3,
        1432.9,
        1350.31,
        2157.09,
        2161.88
      ],
      "errorMs": [
        365.83,
        632.33,
        1426.08,
        1626.5,
        6454.93,
        1121.56
      ],
      "stdDevMs": [
        20.052,
        34.66,
        78.168,
        89.154,
        353.816,
        61.477
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
        64.16,
        178.1,
        271.4,
        416.09,
        436.44,
        659.49
      ],
      "allocMb": [
        84.59,
        134.19,
        131.12,
        181.1,
        322.83,
        247.27
      ],
      "errorMs": [
        108.87,
        295.26,
        97.22,
        98.83,
        44.65,
        136.59
      ],
      "stdDevMs": [
        5.968,
        16.184,
        5.329,
        5.417,
        2.447,
        7.487
      ]
    }
  ]
};
