<template>
  <a-layout class="hb-layout">
    <a-layout-header class="content-layout-header">
      <a-descriptions :column="4">
      <a-descriptions-item label="仓库编号">
          <a-input v-model="query.wareHouseId"></a-input>
        </a-descriptions-item>
        <a-descriptions-item label="物料SKU">
          <a-input v-model="query.sku"></a-input>
        </a-descriptions-item>
        <a-descriptions-item label="条码">
          <a-input v-model="query.barcode"></a-input>
        </a-descriptions-item>
        <a-descriptions-item>
          <a-button type="primary" icon="search" size="small" @click="load">
            {{ $t("searchButton") }}
          </a-button>
        </a-descriptions-item>
        <a-descriptions-item label="储位编码">
          <a-input v-model="query.storageId"></a-input>
        </a-descriptions-item>
      </a-descriptions>
    </a-layout-header>
    <a-layout-content class="hb-layout-content">
      <a-table
        :columns="columns"
        :data-source="data"
        bordered
        :rowKey="(row, index) => row.id"
        size="small"
        :pagination="pagination"
        :loading="loading"
        @change="change"
      >
      </a-table>
    </a-layout-content>
  </a-layout>
</template>
<script>
import { OutSearch } from "@/api/stock/stock";
import locale from "@/localize/materials/localize";
import component from "@/lib/base";
export default {
  name: "OutOfWarehouse",
  mixins: [component],
  i18n: {
    messages: locale,
  },
  components: {},
  data() {
    return {
      loading: false,
      pagination: {
        current: 1,
        pageSize: 10,
        total: 0,
        "show-total": (total) => `共${total}条数据`,
        "show-size-changer": true,
      },
      columns: [
        {
          dataIndex: "barCode", //接口返回该字段为物料SKU编码
          title: this.$t("tableTitle.barCode"),
          align: "center",
        },
        {
          dataIndex: "materialInfoId", //接口返回该字段为物料SKU编码
          title: this.$t("tableTitle.sku"),
          align: "center",
        },
        {
          dataIndex: "name",
          title: this.$t("tableTitle.name"),
          align: "center",
        },
        {
          dataIndex: "batch",
          title: this.$t("tableTitle.batch"),
          align: "center",
        },
        {
          dataIndex: "quatity", //出库数量
          title: this.$t("tableTitle.outQuantity"),
          align: "center",
        },
        {
          dataIndex: "wareHouseId",
          title: this.$t("tableTitle.wareHouseId"),
          align: "center",
        },
        // {
        //     dataIndex: "storageId",
        //     title: this.$t("tableTitle.wareHouseId"),
        //     align: "center",
        // },
        {
          dataIndex: "storeTime",
          title: this.$t("tableTitle.storeTime"),
          align: "center",
        },
        {
          dataIndex: "outputTime",
          title: this.$t("tableTitle.outputTime"),
          align: "center",
        },
      ],
      data: [],
      query: {
        sku: "",
        wareHouseId: "",
        storageId: "",
        barcode: "",
      },
    };
  },
  mounted() {
    this.load();
  },
  methods: {
    load: async function () {
      let _this = this;
      try {
        _this.loading = true;
        let query = Object.assign(
          {
            skipCount: (this.pagination.current - 1) * this.pagination.pageSize,
            maxResultCount: this.pagination.pageSize,
          },
          this.query
        );
        let result = await OutSearch(query);
        _this.data = result.items;
        _this.pagination.total = result.totalCount;
      } catch (err) {
        this.$message.error(err.message);
      } finally {
        this.loading = false;
      }
    },
    change: function (pagination) {
      this.pagination = pagination;
      this.load();
    },
  },
};
</script>
