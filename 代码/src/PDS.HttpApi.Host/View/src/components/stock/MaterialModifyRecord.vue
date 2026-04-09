<template>
  <a-layout class="hb-layout">
    <a-layout-header class="content-layout-header">
      <a-descriptions :column="4">
        <a-descriptions-item label="物料SKU">
          <a-input v-model="query.sku"></a-input>
        </a-descriptions-item>
        <a-descriptions-item label="物料条码">
          <a-input v-model="query.barCode" style="width:280px"></a-input>
        </a-descriptions-item>
        <a-descriptions-item>
          <a-button type="primary" icon="search" size="small" @click="load">
            {{ $t("searchButton") }}
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
        >
        </a-table>
    </a-layout-content>
  </a-layout>
</template>
<script>
import { SearchModifyRecord } from "@/api/stock/stock";
import locale from "@/localize/materials/localize";
import component from "@/lib/base";
export default {
  name: "MaterialModifyRecord",
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
      },
      columns: [
        {
          dataIndex: "barCode",
          title: this.$t("tableTitle.barCode"),
          align: "center",
        },
        {
          dataIndex: "sku",
          title: this.$t("tableTitle.sku"),
          align: "center",
        },
        {
          dataIndex: "oldCount",
          title: this.$t("tableTitle.oldCount"),
          align: "center",
        },
        {
          dataIndex: "newCount",
          title: this.$t("tableTitle.newCount"),
          align: "center",
        },
        {
          dataIndex: "time",
          title: this.$t("tableTitle.time"),
          align: "center",
        },
        {
          dataIndex: "userid",
          title: this.$t("tableTitle.userid"),
          align: "center",
        },
        {
          dataIndex: "modifyType",
          title: this.$t("tableTitle.modifyType"),
          align: "center",
        }
      ],
      data: [],
      showForm: false,
      query: {
        sku: "",
        barCode: "",
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
        let query = Object.assign(this.query, {
          skipCount: (this.pagination.current - 1) * this.pagination.pageSize,
          maxResultCount: this.pagination.pageSize,
        });
        let result = await SearchModifyRecord(query);
        result.items.forEach(item => {
            item.modifyType = item.modifyType == 0? "赢" : "亏"
        });
        _this.data = result.items;

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
    close: function () {
      this.showForm = false;
      this.load();
    },
    show: function () {
      this.showForm = true;
      
    },
  },
};
</script>
