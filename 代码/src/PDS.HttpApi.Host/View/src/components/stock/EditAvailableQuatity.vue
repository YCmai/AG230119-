<template>
  <a-layout class="hb-layout">
    <a-layout-header class="content-layout-header">
      <a-descriptions :column="4">
        <a-descriptions-item label="物料SKU">
          <a-input v-model="query.sku"></a-input>
        </a-descriptions-item>
        <a-descriptions-item label="仓库编号">
          <a-input v-model="query.whereHouseId"></a-input>
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
        <template slot="operation" slot-scope="text, record">
          <a-space>
            <a href="javascript:void(0);" @click="show(record)">
              <a-icon type="edit" />{{ $t("actions.edit") }}
            </a>
          </a-space>
        </template>
        </a-table>
    </a-layout-content>
    <EditAvailableQuatityForm v-if="showForm" ref="editAvailableQuatityForm" @close="close" />
  </a-layout>
</template>
<script>
import EditAvailableQuatityForm from "./Form/EditAvailableQuatityForm.vue";
import { InSearch } from "@/api/stock/stock";
import locale from "@/localize/materials/localize";
import component from "@/lib/base";
export default {
  name: "EditAvailableQuatity",
  mixins: [component],
  i18n: {
    messages: locale,
  },
  components: { EditAvailableQuatityForm },
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
          dataIndex: "availableQuatity",
          title: this.$t("tableTitle.availableQuantity"),
          align: "center",
        },
        {
          dataIndex: "lockedQuatity",
          title: this.$t("tableTitle.lockedQuantity"),
          align: "center",
        },
        {
          dataIndex: "freezeQuatity",
          title: this.$t("tableTitle.freezeQuantity"),
          align: "center",
        },
        {
          dataIndex: "wareHouseId",
          title: this.$t("tableTitle.wareHouseId"),
          align: "center",
        },
        {
          dataIndex: "storageId",
          title: this.$t("tableTitle.storageId"),
          align: "center",
        },
        {
          dataIndex: "storeTime",
          title: this.$t("tableTitle.storeTime"),
          align: "center",
        },
        {
          dataIndex: "actions",
          title: "操作",
          align: "center",
          scopedSlots: { customRender: "operation" },
          width: 200,
        },
      ],
      data: [],
      showForm: false,
      query: {
        sku: "",
        whereHouseId: "",
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
        let result = await InSearch(query);
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
    show: function (input) {
      this.showForm = true;
      if (input)
        this.$nextTick(() => {
          this.$refs.editAvailableQuatityForm.load(input);
        });
    },
  },
};
</script>
