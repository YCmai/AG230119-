<template>
  <a-layout class="hb-layout">
    <a-layout-header class="content-layout-header">
      <a-descriptions :column="4">
        <a-descriptions-item label="入库单号">
          <a-input></a-input>
        </a-descriptions-item>
        <a-descriptions-item label="物料SKU">
          <a-input></a-input>
        </a-descriptions-item>
        <a-descriptions-item>
          <a-button type="primary" icon="search" size="small" @click="load">
            搜索
          </a-button>
        </a-descriptions-item>
      </a-descriptions>
    </a-layout-header>
    <a-layout-content class="content-layout-content">
      <a-table
        :columns="columns"
        :data-source="data"
        bordered
        :rowKey="(row, index) => row.sku"
        size="small"
        :pagination="pagination"
        :loading="loading"
        @change="change"
      >
        <template slot="operation" slot-scope="text, record">
          <a-space>
            <a href="javascript:void(0);" @click="show(record.sku)">
              <a-icon type="edit" />{{ $t("actions.edit") }}
            </a>
            <a href="javascript:void(0);" @click="remove(record.sku)">
              <a-icon type="edit" />{{ $t("actions.delete") }}
            </a>
          </a-space>
        </template>
      </a-table>
    </a-layout-content>
  </a-layout>
</template>
<script>
export default {
  name: "LineInput",
  data() {
    return {
      loading: false,
      pagination: {
        current: 1,
        pageSize: 10,
        total: 0,
      },
      data: [],
      columns: [
        {
          dataIndex: "id",
          title: "入库单编号",
          align: "center",
          width: 200,
        },
        {
          dataIndex: "barcode",
          title: "物料条码",
          align: "center",
        },
        {
          dataIndex: "sku",
          title: "物料SKU",
          align: "center",
        },
        {
          dataIndex: "name",
          title: "物料批次",
          align: "center",
        },
        {
          dataIndex: "inputStorageId",
          title: "储位编码",
          align: "center",
        },
      ],
    };
  },
  methods: {
    load: async function () {},
    change: function (pagination) {},
  },
};
</script>
