<template>
  <a-layout class="hb-layout">
    <a-layout-header class="content-layout-header">
      <a-descriptions style="text-align: left" :column="4">
        <a-descriptions-item label="父级条码">
          <a-input v-model="query.parentBarCode"></a-input>
        </a-descriptions-item>
        <a-descriptions-item label="子级条码">
          <a-input v-model="query.barCode"></a-input>
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
        :rowKey="(row, index) => row.id"
        size="small"
        :pagination="pagination"
        :loading="loading"
        @change="change"
      ></a-table>
    </a-layout-content>
  </a-layout>
</template>
<script>
import { GetList } from "@/api/stock/barcode";
import component from "@/lib/base";
export default {
  name: "BarcodeList",
  mixins: [component],
  data() {
    return {
      pagination: {
        total: 0,
        pageSize: 10,
        current: 1,
        "show-total": (total) => `共${total}条数据`,
        "show-size-changer": true,
      },
      loading: false,
      data: [],
      columns: [
        {
          dataIndex: "fatherBarcode",
          title: "父级条码",
          align: "center",
        },
        {
          dataIndex: "fatherQuantity",
          title: "父级数量",
          align: "center",
        },
        {
          dataIndex: "childBarcode",
          title: "子级条码",
          align: "center",
        },
        {
          dataIndex: "childQuantity",
          title: "子级数量",
          align: "center",
        },
        {
          dataIndex: "time",
          title: "拆分时间",
          align: "center",
        },
      ],
      query: {
        parentBarCode: "",
        barCode: "",
      },
    };
  },
  mounted() {
    this.load();
  },
  methods: {
    change: function (pagination) {
      this.pagination = pagination;
    },
    load: async function () {
      this.loading = true;
      try {
        let query = Object.assign(this.query, {
          skipCount: (this.pagination.current - 1) * this.pagination.pageSize,
          maxResultCount: this.pagination.pageSize,
        });
        let result = await GetList(query);
        this.data = result.items;
        this.pagination.total = result.totalCount;
      } catch (err) {
        this.$message.error(err.message);
      } finally {
        this.loading = false;
      }
    },
  },
};
</script>
